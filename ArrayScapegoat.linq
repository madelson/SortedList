<Query Kind="Program" />

void Main()
{	
	ShouldEqual(LeftChild(1, 1), 0);
	ShouldEqual(LeftChild(3, 2), 1);
	ShouldEqual(LeftChild(5, 1), 4);
	ShouldEqual(LeftChild(7, 3), 3);
	ShouldEqual(LeftChild(13, 1), 12);
	
	ShouldEqual(RightChild(1, 1), 2);
	ShouldEqual(RightChild(3, 2), 5);
	ShouldEqual(RightChild(5, 1), 6);
	ShouldEqual(RightChild(7, 3), 11);
	ShouldEqual(RightChild(11, 2), 13);
	
	ShouldEqual(Parent(0, 0), 1);
	ShouldEqual(Parent(1, 1), 3);
	ShouldEqual(Parent(4, 0), 5);
	ShouldEqual(Parent(3, 2), 7);
	ShouldEqual(Parent(12, 0), 13);
	ShouldEqual(Parent(2, 0), 1);
	ShouldEqual(Parent(5, 1), 3);
	ShouldEqual(Parent(6, 0), 5);
	ShouldEqual(Parent(11, 2), 7);
	ShouldEqual(Parent(13, 1), 11);

	ShouldEqual(SL<string>.BalancedPositionIterator(root: 7, count: 10).SequenceEqual(new[] { 0, 1, 2, 3, 4, 5, 7, 9, 11, 13 }), true);
	ShouldEqual(SL<string>.BalancedPositionIterator(root: 3, count: 1).SequenceEqual(new[] { 3 }), true);
	ShouldEqual(SL<string>.BalancedPositionIterator(root: 3, count: 3).SequenceEqual(new[] { 1, 3, 5 }), true);

	var sl = new SL<string>();
	sl.Insert("d");
	sl.Insert("f");
	sl.Insert("b");
	sl.Insert("g");
	sl.Insert("a");
	sl.Insert("c");
	sl.Insert("e");
	ShouldEqual(sl._entries.Select(e => e.Value).SequenceEqual(new[] { null, "a", null, "b", null, "c", null, "d", null, "e", null, "f", null, "g", null }), true);

	foreach (var ch in "hijklmnopqrstuvwxyz") { sl.Insert(ch.ToString()); }
	sl.BalanceCost.Dump();
	sl._entries.Dump();
}

static void ShouldEqual<T>(T actual, T expected)
{
	if (!EqualityComparer<T>.Default.Equals(actual, expected))
	{
		throw new InvalidOperationException($"Expected '{expected}'. Was '{actual}'");
	}
}

// for 7-length
// 0, 1, 2, 3, 4, 5, 6
// (1, rd=1) => 0 (001 => 000)
// (3, rd=2) => 1 (011 => 001)
// (5, rd=1) => 4 (101 => 100)
// else => -1

// for 15-length
// 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14
// (7, rd=3) => 3 (0111 => 0011)
// (11, rd=2) => 9 (1011 => 1001)
static int LeftChild(int current, int remainingDepth)
{
	return current & ~(1 << (remainingDepth - 1));
}

// (1, rd=1) => 2 (001 => 010)
// (3, rd=2) => 5 (011 => 101)
// (5, rd=1) => 6 (101 => 110)
// (7, rd=3) => 11 (0111 => 1011)
// (11, rd=2) => 13 (1011 => 1101)
static int RightChild(int current, int remainingDepth)
{
	return (current | (1 << remainingDepth)) & ~(1 << (remainingDepth - 1));
}

static int Parent(int current, int remainingDepth)
{
	return (current & ~(1 << (remainingDepth + 1))) | (1 << remainingDepth);
}

class SL<T>
{
	public (T Value, int Count)[] _entries = new (T Value, int Count)[1];
	public int _maxDepth = 0;
	
	private int Root => this._entries.Length >> 1;

	public void Insert(T value)
	{
		var current = this.Root;
		var remainingDepth = this._maxDepth;
		while (this._entries[current].Count != 0)
		{
			if (remainingDepth == 0)
			{
				// todo should we really grow here?
				this.GrowAndRebalance();
				this.Insert(value);
				return;
				// throw new ArgumentException("needs balance at " + current);
			}
			
			var cmp = Comparer<T>.Default.Compare(value, this._entries[current].Value);
			if (cmp < 0)
			{
				current = LeftChild(current, remainingDepth--);
			}
			else if (cmp > 0)
			{
				current = RightChild(current, remainingDepth--);
			}
			else
			{
				throw new ArgumentException($"duplicate value {value}");
			}
		}

		this._entries[current] = (value, 1);
		while (remainingDepth < this._maxDepth)
		{
			current = Parent(current, remainingDepth++);
			this._entries[current].Count++;
		}
	}
	
	public int BalanceCost = 0;

	private void GrowAndRebalance()
	{
		var count = this._entries[this.Root].Count;
		this.BalanceCost += count;
		
		var oldEntries = this._entries;
		this._entries = new (T Value, int Count)[(this._entries.Length * 2) + 1];
		++this._maxDepth;

		var i = 0;
		foreach (var pos in BalancedPositionIterator(root: this.Root, count: count))
		{
			while (oldEntries[i].Count == 0) { ++i; }
			this._entries[pos] = (Value: oldEntries[i].Value, Count: 1); // todo count is wrong
			++i;
		}

		// fix counts (we shouldn't need to do this after the fact)
		void FixCount(int j, int remainingDepth)
		{
			if (remainingDepth > 1)
			{
				FixCount(LeftChild(j, remainingDepth), remainingDepth - 1);
				FixCount(RightChild(j, remainingDepth), remainingDepth - 1);
			}
			if (remainingDepth > 0)
			{
				this._entries[j].Count += this._entries[LeftChild(j, remainingDepth)].Count + this._entries[RightChild(j, remainingDepth)].Count;
			}
		}
		FixCount(this.Root, this._maxDepth);
		ShouldEqual(this._entries[this.Root].Count, count);
		this._entries.Dump();
	}
			
	public static IEnumerable<int> BalancedPositionIterator(int root, int count)
	{
		const int RightChildOfParent = 0, LeftChildOfParent = 1, ParentOfRightChild = 2, ParentOfLeftChild = 3;
		
		if (count == 0) { yield break; }
		
		var maxDepth = (int)Math.Log(count, 2);
		int countAtBottomLayer = 1;
		for (var i = 1; i < count; i = (2 * i) + 1)
		{
			countAtBottomLayer = count - i;
		}

		var remaining = count;
		// TODO bug we're using this for right/left child calcs but we really shouldn't be because they need the absolute remainingDepth
		var remainingAtBottomLayer = countAtBottomLayer;
		var current = root;
		var currentRemainingDepth = maxDepth;
		//var isRightChild = true;
		var state = RightChildOfParent;
		var minDepth = 0;
		while (true)
		{
			switch (state)
			{
				case RightChildOfParent:
					if (currentRemainingDepth > minDepth)
					{
						do { current = LeftChild(current, currentRemainingDepth--); }
						while (currentRemainingDepth > minDepth);
						state = LeftChildOfParent;
					}
					else
					{
						// act as if we just came to this node from it's left child
						goto case ParentOfLeftChild;
					}
					break;
				case LeftChildOfParent:
					yield return current;
					--remaining;
					if (remaining == 0) { yield break; }
					if (currentRemainingDepth == minDepth)
					{
						if (--remainingAtBottomLayer == 0) { minDepth = 1; }
						current = Parent(current, currentRemainingDepth++);
						state = ParentOfLeftChild;
					}
					else
					{
						current = RightChild(current, currentRemainingDepth--);
						state = RightChildOfParent;
					}
					break;
				case ParentOfRightChild:
					{
						// after coming back to parent from the right, we've already yielded the
						// value so just continue
						var parent = Parent(current, currentRemainingDepth++);
						state = parent < current ? ParentOfRightChild : ParentOfLeftChild;
						current = parent;
						break;
					}
				case ParentOfLeftChild:
					//goto case LeftChildOfParent;
					// after coming back to parent from the left, yield the current value
					// and then go right
					yield return current;
					--remaining;
					if (remaining == 0) { yield break; }
					if (currentRemainingDepth == minDepth)
					{
						if (--remainingAtBottomLayer == 0) { minDepth = 1; }
						var parent = Parent(current, currentRemainingDepth++);
						state = parent < current ? ParentOfRightChild : ParentOfLeftChild;
						current = parent;
					}
					else
					{
						current = RightChild(current, currentRemainingDepth--);
						state = RightChildOfParent;
					}
					break;
			}
			
//			if (isRightChild) // goto min
//			{
//				while (currentRemainingDepth > minDepth) { current = LeftChild(current, currentRemainingDepth--); }
//				isRightChild = false;
//			}
//			else
//			{
//				yield return current;
//				--remaining;
//				if (remaining == 0) { yield break; }
//
//				if (currentRemainingDepth == minDepth)
//				{
//					if (currentRemainingDepth == 0)
//					{
//						--remainingAtBottomLayer;
//						if (remainingAtBottomLayer == 0) { minDepth = 1; }
//					}
//					current = Parent(current, currentRemainingDepth++); 
//				}
//				else
//				{
//					current = RightChild(current, currentRemainingDepth--);
//					isRightChild = true;
//				}
//			}
		}
	}
}