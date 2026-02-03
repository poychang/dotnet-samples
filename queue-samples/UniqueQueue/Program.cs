using UniqueQueue;

var uq = new UniqueQueue<string>();
uq.Enqueue("A");
uq.Enqueue("B");
uq.Enqueue("C");
uq.Enqueue("B"); // false，不加入
uq.Enqueue("D");
while (uq.Count > 0)
    Console.WriteLine(uq.Dequeue());

Console.WriteLine();

var uq2 = new UniqueQueue<Demo>(new UniqueQueueComparer<Demo>());
uq2.Enqueue(new Demo { Name = "Alice" });
uq2.Enqueue(new Demo { Name = "Bob" });
uq2.Enqueue(new Demo { Name = "Alice" }); // false，不加入
while (uq2.Count > 0)
    Console.WriteLine(uq2.Dequeue().Name);
Console.WriteLine("\nAdd to queue again.");
uq2.Enqueue(new Demo { Name = "Alice" });
while (uq2.Count > 0)
    Console.WriteLine(uq2.Dequeue().Name);

class Demo
{
    public string Name { get; set; } = string.Empty;
}
