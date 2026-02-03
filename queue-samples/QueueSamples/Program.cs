// 建立 Queue
Queue<string> queue = new Queue<string>();

// 入列（Enqueue）
queue.Enqueue("A");
queue.Enqueue("B");
queue.Enqueue("C");

// 查看目前最前端元素（Peek，不移除）
Console.WriteLine("\nPeek:");
Console.WriteLine(queue.Peek()); // A

// 出列（Dequeue）
Console.WriteLine("\nDequeue:");
Console.WriteLine(queue.Dequeue()); // A
Console.WriteLine(queue.Dequeue()); // B

// 再次入列
queue.Enqueue("D");

// 逐一取出直到清空
Console.WriteLine("\nPurge the queue:");
while (queue.Count > 0)
{
    Console.WriteLine(queue.Dequeue());
}
