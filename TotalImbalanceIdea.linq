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
	// idea: keep track of total absolute imbalance. When it gets too high (how high?), we should try to improve things on the way back (or should we always do this?)
	
	var tree = new Tree<int>();
	Enumerable.Range(1, 3).ToList().ForEach(i => tree.Add(i));
	tree.Dump();
	tree.CheckInvariants();
}

public class Tree<TKey>
{
	public readonly IComparer<TKey> Comparer = Comparer<TKey>.Default;
	public Node<TKey> Root;
	public long TotalImbalance;
	
	public void Add(TKey key)
	{
		this.Add(key, ref this.Root, 0);
	}
	
	private void Add(TKey key, ref Node<TKey> node, int totalImbalance)
	{
		if (node == null) 
		{ 
			node = new Node<TKey> { Key = key, Count = 1 };
			this.TotalImbalance += totalImbalance;
			return;
		}

		var cmp = this.Comparer.Compare(key, node.Key);
		if (cmp > 0)
		{
			this.Add(key, ref node.Left, totalImbalance + (node.LeftCount >= node.RightCount ? 1 : -1));
		}
		else
		{
			this.Add(key, ref node.Right, totalImbalance + (node.RightCount >= node.LeftCount ? 1 : -1));
		}
		++node.Count;

		var (shouldRotate, left, diff) = ShouldRotate(node);
		if (shouldRotate)
		{
			if (left)
			{
				Node<TKey>.RotateLeft(ref node);
			}
			else
			{
				Node<TKey>.RotateRight(ref node);
			}
			this.TotalImbalance -= diff;
		}
	}

	private (bool shouldRotate, bool left, int diff) ShouldRotate(Node<TKey> node)
	{
		if (node.Count < 3) { return (false, false, 0); }		

		var leftCount = node.LeftCount;
		var rightCount = node.RightCount;
		var leftRightCount = node.Left?.RightCount ?? 0;
		var rightLeftCount = node.Right?.LeftCount ?? 0;
		new { leftCount, rightCount, leftRightCount, rightLeftCount }.Dump();
		
		if (leftCount == 0) { return (true, true, rightCount
		
		var currentImbalance = Abs(leftCount - rightCount)
			+ Abs(leftCount - 1 - (leftRightCount << 1))
			+ Abs(rightCount - 1 - (rightLeftCount << 1));
		currentImbalance.Dump();

		if (rightCount > 0)
		{
			var leftRotateImbalance = Abs(leftCount - rightCount - 2 - (leftRightCount << 1))
				+ Abs(leftRightCount - rightCount);
			leftRotateImbalance.Dump();
			if (leftRotateImbalance < currentImbalance) { return (true, true, currentImbalance - leftRotateImbalance); }
		}

		if (leftCount > 0)
		{
			var rightRotateImbalance = Abs(rightCount - leftCount - 2 - (rightLeftCount << 1))
				+ Abs(leftCount - rightLeftCount);
			rightRotateImbalance.Dump();
			if (rightRotateImbalance < currentImbalance) { return (true, false, currentImbalance - rightRotateImbalance); }
		}
		
		return (false, false, 0);
	}

	public void CheckInvariants()
	{
		var totalImbalance = 0L;
		
		void CheckInvariants(Node<TKey> node)
		{
			if (node == null) { return; }

			var expectedCount = node.LeftCount + 1 + node.RightCount;
			if (node.Count != expectedCount) { throw new Exception($"Bad count at {node}: expected {expectedCount}. Was {node.Count}"); }
			
			totalImbalance += Abs(node.LeftCount - node.RightCount);
			CheckInvariants(node.Left);
			CheckInvariants(node.Right);
		}
		
		CheckInvariants(this.Root);

		if (totalImbalance != this.TotalImbalance)
		{
			throw new Exception($"Bad total imbalance: expected {totalImbalance}. Was {this.TotalImbalance}");
		}
	}
}

static int Abs(int value) => Math.Abs(value);

// Define other methods and classes here
public class Node<TKey>
{
	public TKey Key;
	public int Count;
	public Node<TKey> Left, Right;

	public int LeftCount => this.Left?.Count ?? 0;
	public int RightCount => this.Right?.Count ?? 0;
	public void RecalculateCount() => this.Count = this.LeftCount + this.RightCount + 1;

	public static void RotateLeft(ref Node<TKey> node)
	{
		var newRoot = node.Right;
		node.Right = newRoot.Left;
		newRoot.Left = node;
		newRoot.RecalculateCount();
		node.RecalculateCount();
		node = newRoot;
	}

	public static void RotateRight(ref Node<TKey> node)
	{
		var newRoot = node.Left;
		node.Left = newRoot.Right;
		newRoot.Right = node;
		newRoot.RecalculateCount();
		node.RecalculateCount();
		node = newRoot;
	}
}