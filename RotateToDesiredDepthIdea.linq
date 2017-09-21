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
	Node node = null;
	Node.Add(ref node, "a");
	Node.Add(ref node, "b");
	Node.Add(ref node, "c");
	Node.Add(ref node, "d");
	Node.Add(ref node, "e");
	node.Dump();
}

// Define other methods and classes here
class Node
{
	public string Key;
	public int Count;
	public Node Left, Right;

	public int LeftCount => this.Left?.Count ??0;
	public int RightCount => this.Right?.Count ??0;
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

	public static int Add(ref Node node, string key, int outerLeftCount = 0, int depth = 0)
	{
		if (node == null)
		{
			node = new Node { Key = key, Count = 1 };
			
			var oneBasedIndex= outerLeftCount + 1;
			var desiredDepth = 0;
			while (oneBasedIndex > 1)
			{
				++desiredDepth;
				oneBasedIndex >>= 1;
			}

			$"inserting '{key}' at depth {depth}. Desired depth is {desiredDepth}. Index is {outerLeftCount}".Dump();
			return desiredDepth;
		}

		var cmp = string.Compare(key, node.Key);
		if (cmp < 0)
		{
			var desiredDepth = Add(ref node.Left, key, outerLeftCount, depth + 1);
			if (desiredDepth <= depth)
			{
				RotateRight(ref node);
			}
			return desiredDepth;
		}
		else
		{
			var desiredDepth = Add(ref node.Right, key, outerLeftCount + 1 + node.LeftCount, depth + 1);
			if (desiredDepth <= depth)
			{
				RotateLeft(ref node);
			}
			return desiredDepth;
		}
	}
}