using Octokit;
using System.Net.Http.Headers;
using System.Text.Json;

const string clientId = "<YOUR_GITHUB_OAUTH_APP_CLIENT_ID>"; // OAuth App Client ID
const string scope = "public_repo";           // 只使用 public_repo 權限
const string owner = "<OWNER>";               // 擁有者
const string repo = "<REPO_NAME>";            // 儲存庫名稱
const string branch = "<BRANCH_NAME>";        // 目標分支名稱，例如 main 或 master

// 1) Device Flow 取得使用者 Access Token
string token = await GetAccessTokenWithDeviceFlowAsync(clientId, scope);

// 2) 用 Octokit 帶 token
var gh = new GitHubClient(new Octokit.ProductHeaderValue("PublicRepoConsole"))
{
    Credentials = new Credentials(token)
};

// === 直接修改（需要你對 repo 有寫入權） ===
//await UpsertFileAsync(gh, owner, repo, branch, "docs/hello.txt", "Hello from OAuth Device Flow!\n", "feat: add hello.txt via console");

// === 刪檔 ===
//await DeleteFileAsync(gh, owner, repo, branch, "docs/hello.txt", "chore: remove old.txt");

// === 如果你沒有寫入權，走 Fork + PR ===
// await ForkAndPrAsync(gh, owner, repo, upstreamBranch: "main",
//     newPath: "contrib/new-file.txt", newContent: "contrib via device flow\n",
//     commitMessage: "feat: add contrib file", prTitle: "Add contrib file", prBody: "Automated by console app");



// ---------------- Device Flow ----------------
static async Task<string> GetAccessTokenWithDeviceFlowAsync(string clientId, string scope)
{
    using var http = new HttpClient();

    // Step 1: 取得 device_code
    var req1 = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/device/code");
    req1.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    req1.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["client_id"] = clientId,
        ["scope"] = scope
    });
    var resp1 = await http.SendAsync(req1);
    var resp1Content = await resp1.Content.ReadAsStringAsync();
    var payload1 = JsonDocument.Parse(resp1Content).RootElement;

    string deviceCode = payload1.GetProperty("device_code").GetString()!;
    string userCode = payload1.GetProperty("user_code").GetString()!;
    string verifyUri = payload1.GetProperty("verification_uri").GetString()!;
    int interval = payload1.GetProperty("interval").GetInt32();

    Console.WriteLine("請在瀏覽器開啟以下網址並輸入代碼完成授權：");
    Console.WriteLine(verifyUri);
    Console.WriteLine($"User Code: {userCode}");

    // Step 2: 輪詢換取 access_token
    while (true)
    {
        await Task.Delay(TimeSpan.FromSeconds(interval));

        var req2 = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token");
        req2.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req2.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["device_code"] = deviceCode,
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code"
        });

        var resp2 = await http.SendAsync(req2);
        var payload2 = JsonDocument.Parse(await resp2.Content.ReadAsStringAsync()).RootElement;

        if (payload2.TryGetProperty("access_token", out var tokenProp))
        {
            var accessToken = tokenProp.GetString()!;
            Console.WriteLine("授權成功 ✅");
            return accessToken;
        }

        if (payload2.TryGetProperty("error", out var err))
        {
            var errStr = err.GetString();
            if (errStr == "authorization_pending") continue;         // 尚未確認，持續輪詢
            if (errStr == "slow_down") { interval += 5; continue; }   // 放慢
            throw new Exception($"OAuth error: {errStr}");
        }
    }
}

// ---------------- 直接對 repo 修改檔案（需寫入權） ----------------
static async Task UpsertFileAsync(
    GitHubClient gh, string owner, string repo, string branch,
    string path, string content, string commitMessage)
{
    // 試著取得現有檔案的 SHA（存在 → Update；不存在 → Create）
    string? sha = null;
    try
    {
        var file = await gh.Repository.Content.GetAllContentsByRef(owner, repo, path, branch);
        sha = file[0].Sha;
    }
    catch (NotFoundException) { /* 不存在，走 Create */ }

    if (sha is null)
    {
        var create = new CreateFileRequest(commitMessage, content, branch);
        await gh.Repository.Content.CreateFile(owner, repo, path, create);
        Console.WriteLine($"Created: {path}");
    }
    else
    {
        var update = new UpdateFileRequest(commitMessage, content, sha, branch);
        await gh.Repository.Content.UpdateFile(owner, repo, path, update);
        Console.WriteLine($"Updated: {path}");
    }
}

static async Task DeleteFileAsync(
    GitHubClient gh, string owner, string repo, string branch,
    string path, string commitMessage)
{
    var file = await gh.Repository.Content.GetAllContentsByRef(owner, repo, path, branch);
    var del = new DeleteFileRequest(commitMessage, file[0].Sha, branch);
    await gh.Repository.Content.DeleteFile(owner, repo, path, del);
    Console.WriteLine($"Deleted: {path}");
}

// ---------------- 沒寫入權 → Fork + PR ----------------
static async Task ForkAndPrAsync(
    GitHubClient gh, string upstreamOwner, string upstreamRepo, string upstreamBranch,
    string newPath, string newContent, string commitMessage,
    string prTitle, string prBody)
{
    // 1) 先 fork
    var fork = await gh.Repository.Forks.Create(upstreamOwner, upstreamRepo, new NewRepositoryFork());

    // 2) 取得預設分支最新 SHA，從上游建新分支名稱
    string featureBranch = $"feat/{DateTime.UtcNow:yyyyMMddHHmmss}";
    var upstreamRef = await gh.Git.Reference.Get(upstreamOwner, upstreamRepo, $"heads/{upstreamBranch}");

    // 在 fork 上建立同名分支（指向上游同一個 commit）
    await gh.Git.Reference.Create(fork.Owner.Login, fork.Name, new NewReference($"refs/heads/{featureBranch}", upstreamRef.Object.Sha));

    // 3) 在 fork 的分支上新增檔案
    var create = new CreateFileRequest(commitMessage, newContent, featureBranch);
    await gh.Repository.Content.CreateFile(fork.Owner.Login, fork.Name, newPath, create);

    // 4) 從 fork 發 PR 回上游
    var pr = new NewPullRequest(prTitle, $"{fork.Owner.Login}:{featureBranch}", upstreamBranch)
    {
        Body = prBody
    };
    var prResp = await gh.PullRequest.Create(upstreamOwner, upstreamRepo, pr);

    Console.WriteLine($"PR 建立完成：#{prResp.Number} {prResp.HtmlUrl}");
}

// You can define other methods, fields, classes and namespaces here
