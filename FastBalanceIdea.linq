<Query Kind="Program">
  <Reference Relative="..\OData\MedallionOData\MedallionOData\bin\Debug\Newtonsoft.Json.dll">&lt;MyDocuments&gt;\Interests\CS\OData\MedallionOData\MedallionOData\bin\Debug\Newtonsoft.Json.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Collections.ObjectModel</Namespace>
  <Namespace>System.Collections.Specialized</Namespace>
  <Namespace>System.Runtime.Remoting.Messaging</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
//	for (var i = 1; i < 1000000; ++i)
//	{
//		if (FloorLogB(i) != (int)Math.Log(i, 2))
//		{
//			i.Dump();
//		}
//	}
	MyExtensions.Time(() =>
{
	var s = 0; for (var i = 0; i < 20000; ++i) { s += FloorLogB(i); }
	if (s == int.MaxValue) { throw new Exception(); }
}).Dump();
	MyExtensions.Time(() =>
	{
		var s = 0; for (var i = 0; i < 20000; ++i) { s += FloorLog(i); }
		if (s == int.MaxValue) { throw new Exception(); }
	}).Dump();

	Node node = null;
	Node.Add(ref node, "a");
	Node.Add(ref node, "b");
	Node.Add(ref node, "c");
	Node.Add(ref node, "d");
	Node.Add(ref node, "e");
	Node.Add(ref node, "f");
	Node.Add(ref node, "g");

	Node.Balance(ref node);
	Node.Balance(ref node);

	using (var e = Node.GetEnumerator(node))
	{
		while (e.MoveNext()) { e.Current.Key.Dump(); }
	}
	
	node.Dump();
}

// Define other methods and classes here
class Node
{
	public string Key;
	public int Count;
	public Node Left, Right;

	public int LeftCount => this.Left?.Count ?? 0;
	public int RightCount => this.Right?.Count ?? 0;
	public void RecalculateCount() => this.Count = this.LeftCount + this.RightCount + 1;

	public override string ToString()
	{
		var sb = new StringBuilder();

		void ToString(Node node, int depth)
		{
			if (depth > 0) { sb.AppendLine(); }
			sb.Append('\t', depth);
			if (node != null)
			{
				sb.Append(node.Key);
				if (node.Left != null || node.Right != null)
				{
					sb.Append('(');
					ToString(node.Left, depth + 1);
					sb.Append(',');
					ToString(node.Right, depth + 1);
					sb.Append('\t', depth).Append(')');
				}
			}
			else { sb.Append("null"); }
		}

		ToString(this, depth: 0);
		return sb.ToString();
	}

	public static IEnumerator<Node> GetEnumerator(Node node)
	{
		if (node == null) { yield break; }

		var stack = new Stack<Node>();

		void Load(Stack<Node> s, Node n)
		{
			for (var next = n; next != null; next = next.Left)
			{
				s.Push(next);
			}
		}

		Load(stack, node);
		do
		{
			var current = stack.Pop();
			yield return current;
			Load(stack, current.Right);
		}
		while (stack.Count > 0);
	}
	
	// for log, use http://graphics.stanford.edu/~seander/bithacks.html#IntegerLogLookup

	public static void RotateLeft(ref Node node)
	{
		var newRoot = node.Right;
		node.Right = newRoot.Left;
		newRoot.Left = node;
		newRoot.RecalculateCount();
		node.RecalculateCount();
		node = newRoot;
	}

	public static void RotateRight(ref Node node)
	{
		var newRoot = node.Left;
		node.Left = newRoot.Right;
		newRoot.Right = node;
		newRoot.RecalculateCount();
		node.RecalculateCount();
		node = newRoot;
	}

	public static void Add(ref Node node, string key)
	{
		if (node == null)
		{
			node = new Node { Key = key, Count = 1 };
			return;
		}

		++node.Count; 
		var cmp = string.Compare(key, node.Key);
		if (cmp < 0) { Add(ref node.Left, key); }
		else { Add(ref node.Right, key); }
	}
	
	public static void Balance(ref Node node)
	{
		if (node == null) { return; }
		
		using (var enumerator = GetEnumerator(node))
		{
			enumerator.MoveNext();
			node = Balance(enumerator, node.Count);
		}
	}

	private static Node Balance(IEnumerator<Node> enumerator, int count)
	{
		if (count == 0)
		{
			return null;
		}
		if (count == 1)
		{
			var leaf = enumerator.Current;
			enumerator.MoveNext();
			leaf.Left = leaf.Right = null;
			leaf.Count = 1;
			return leaf;
		}	
		
		var subtreeTotal = count - 1;
		var rightSubtreeTotal = subtreeTotal >> 1;
		var left = Balance(enumerator, subtreeTotal - rightSubtreeTotal);
		var root = enumerator.Current;
		enumerator.MoveNext();
		var right = Balance(enumerator, rightSubtreeTotal);
		root.Left = left;
		root.Right = right;
		root.Count = left.Count + 1 + (right?.Count ?? 0);
		return root;
	}
}

static readonly int[] LogTable256 = new Func<int[]>(() =>
{
	var result = new int[256];
	result[0] = result[1] = 0;
	for (int i = 2; i < 256; i++)
	{
		result[i] = 1 + result[i / 2];
	}
	result[0] = -1; // if you want log(0) to return -1
	
	return result;
})();

static int FloorLog(int v)
{
	var tt = v >> 16;
	if (tt != 0)
	{
		var t = tt >> 8;
		return t != 0 ? 24 + LogTable256[t] : 16 + LogTable256[tt];
	}
	
	var z = v >> 8;
	return z != 0 ? 8 + LogTable256[z] : LogTable256[v];
}

static readonly sbyte[] LogTable256b = new Func<sbyte[]>(() =>
{
	var result = new sbyte[256];
	result[0] = result[1] = 0;
	for (int i = 2; i < 256; i++)
	{
		result[i] = (sbyte)(1 + result[i / 2]);
	}
	result[0] = -1; // if you want log(0) to return -1

	return result;
})();

static int FloorLogB(int v)
{
	var tt = v >> 16;
	if (tt != 0)
	{
		var t = tt >> 8;
		return t != 0 ? 24 + LogTable256b[t] : 16 + LogTable256b[tt];
	}

	var z = v >> 8;
	return z != 0 ? 8 + LogTable256b[z] : LogTable256b[v];
}

private static int log2(int value)
{
	//Contract.Requires(value>0)
	int c = 0;
	while (value > 0)
	{
		c++;
		value >>= 1;
	}
	return c;
}