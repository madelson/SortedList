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
	
}

// idea: we can do an in-place balance by doing 2 passes.
// 1st pass labels each element with it's eventual position using the count field (~ the number to allow 0 to fit)
// 2nd pass is linear through the region. When we hit an element with a negative value we move it into position; if that
// would hit another such element we continue processing the chain. Once we've exhausted the chain we return to our original
// iterator

class ArrayGoat<T>
{
	public (T Value, int Count)[] _entries = new(T Value, int Count)[15];

	public void InPlaceBalance(int root, int rootRemainingDepth, int count, T inserting, int insertingIndex)
	{
		// get the starting index
		var startIndex = root;
		for (var d = rootRemainingDepth; d > 0; --d)
		{
			startIndex = LeftChild(root, d);
		}

		var context = (inserting: inserting, insertingIndex: insertingIndex, iteratorIndex: startIndex, count: count);
	}
	
	private void InPlaceBalance(int current, int remainingDepth, int subTreeCount, ref (T inserting, int insertingIndex, int iteratorIndex, int count) state)
	{
		
	}
}

static int LeftChild(int current, int remainingDepth)
{
	return current & ~(1 << (remainingDepth - 1));
}

static int RightChild(int current, int remainingDepth)
{
	return (current | (1 << remainingDepth)) & ~(1 << (remainingDepth - 1));
}

static int Parent(int current, int remainingDepth)
{
	return (current & ~(1 << (remainingDepth + 1))) | (1 << remainingDepth);
}
