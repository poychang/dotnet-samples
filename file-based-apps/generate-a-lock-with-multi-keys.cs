using System.Security.Cryptography;
using System.Text;

const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 避免易混淆的字元，讓金鑰可讀又穩定

// Configure how many distinct keys we want to derive for the lock.
const int desiredKeyCount = 3;
var lockCode = args.Length > 0 ? args[0] : "acb";

if (string.IsNullOrWhiteSpace(lockCode))
{
    Console.WriteLine("Lock cannot be empty.");
    return;
}

var keyLength = Math.Max(6, lockCode.Length); // 至少 6 碼，避免太容易猜測
var modulus = desiredKeyCount; // 以需要的金鑰數量作為模數：同餘即代表可解鎖
var lockSignature = ComputeSignature(lockCode, modulus); // 將 lock 轉成簽名供比對
var keys = GenerateKeys(lockCode, keyLength, lockSignature, modulus, desiredKeyCount).ToArray();

Console.WriteLine($"Lock pattern        : {lockCode}");
Console.WriteLine($"Signature (mod {modulus}) : {lockSignature}");
Console.WriteLine("Keys that open lock :");
foreach (var key in keys)
{
    Console.WriteLine($" - {key}");
}

Console.WriteLine();
Console.WriteLine("Validation attempts :");
var invalidAttempt = new string('X', Math.Max(1, keyLength - 1));
foreach (var attempt in keys.Concat(new[] { invalidAttempt }))
{
    var result = CanUnlock(attempt, keyLength, lockSignature, modulus)
        ? "[OK]"
        : "[X]";
    Console.WriteLine($" {result} {attempt}");
}

static IEnumerable<string> GenerateKeys(string lockCode, int keyLength, int targetSignature, int modulus, int required)
{
    var result = new List<string>(required);
    var seen = new HashSet<string>(StringComparer.Ordinal);
    var iteration = 0;
    // 利用 lock + iteration 的雜湊作為高熵來源，再微調最後一碼貼齊簽名
    while (result.Count < required)
    {
        var candidate = DeriveCandidate(lockCode, keyLength, iteration++);
        var hardened = EnsureSignature(candidate, targetSignature, modulus);
        if (seen.Add(hardened))
        {
            result.Add(hardened);
        }
    }

    return result;
}

static string DeriveCandidate(string lockCode, int keyLength, int iteration)
{
    var material = Encoding.UTF8.GetBytes($"{lockCode}:{iteration}");
    var hash = SHA256.HashData(material); // 以 SHA-256 提供可預測但難逆推的序列
    return ProjectHashToKey(hash, keyLength);
}

static string ProjectHashToKey(ReadOnlySpan<byte> hash, int keyLength)
{
    var buffer = new char[keyLength];
    for (var i = 0; i < keyLength; i++)
    {
        buffer[i] = Alphabet[hash[i % hash.Length] % Alphabet.Length]; // 直接投影到客製字母表
    }

    return new string(buffer);
}

static string EnsureSignature(string candidate, int targetSignature, int modulus)
{
    var currentSignature = ComputeSignature(candidate, modulus);
    if (currentSignature == targetSignature)
    {
        return candidate;
    }

    var chars = candidate.ToCharArray();
    var sumWithoutLast = 0;
    for (var i = 0; i < chars.Length - 1; i++)
    {
        sumWithoutLast = (sumWithoutLast + chars[i]) % modulus; // 只調整最後一碼，避免破壞前段亂數性
    }

    foreach (var option in Alphabet)
    {
        var nextSignature = (sumWithoutLast + option) % modulus;
        if (nextSignature == targetSignature)
        {
            chars[^1] = option;
            return new string(chars);
        }
    }

    for (var option = (char)33; option <= 126; option++)
    {
        var nextSignature = (sumWithoutLast + option) % modulus;
        if (nextSignature == targetSignature)
        {
            chars[^1] = option;
            return new string(chars);
        }
    }

    return candidate;
}

static int ComputeSignature(string input, int modulus)
{
    var sum = 0;
    foreach (var ch in input)
    {
        sum = (sum + ch) % modulus; // 將字元碼累加後取模，產生 lock 金鑰空間內的簽名
    }

    return sum;
}

static bool CanUnlock(string key, int expectedLength, int lockSignature, int modulus)
{
    if (key.Length != expectedLength)
    {
        return false;
    }

    return ComputeSignature(key, modulus) == lockSignature;
}
