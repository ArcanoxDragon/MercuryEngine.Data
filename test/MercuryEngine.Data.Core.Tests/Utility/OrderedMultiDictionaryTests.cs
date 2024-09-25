using System.Diagnostics.CodeAnalysis;
using MercuryEngine.Data.Core.Utility;

namespace MercuryEngine.Data.Core.Tests.Utility;

[SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments", Justification = "Easier to read and write tests")]
[SuppressMessage("Style", "IDE0028:Simplify collection initializer", Justification = "Easier to see what code is being tested")]
[SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer", Justification = "Easier to see what code is being tested")]
public class OrderedMultiDictionaryTests
{
	#region Add

	[Test]
	public void TestSimpleAdd()
	{
		var dictionary = new OrderedMultiDictionary<string, int>();

		dictionary.Add("apples", 3);

		Assert.Multiple(() => {
			Assert.That(dictionary.Keys, Is.EqualTo(new[] { "apples" }));
			Assert.That(dictionary.Values, Is.EqualTo(new[] { 3 }));
		});
	}

	[Test]
	public void TestAddMultipleDifferentKeys()
	{
		var dictionary = new OrderedMultiDictionary<string, int>();

		dictionary.Add("apples", 3);
		dictionary.Add("bananas", 5);

		Assert.Multiple(() => {
			Assert.That(dictionary.Keys, Has.Count.EqualTo(2));
			Assert.That(dictionary.Keys, Is.EquivalentTo(new[] { "apples", "bananas" }));
			Assert.That(dictionary.Values, Is.EqualTo(new[] { 3, 5 }));
		});
	}

	[TestCase(DuplicateKeyHandlingMode.LowestIndexTakesPriority)]
	[TestCase(DuplicateKeyHandlingMode.HighestIndexTakesPriority)]
	[TestCase(DuplicateKeyHandlingMode.PreventDuplicateKeys)]
	public void TestAddMultipleSameKey(DuplicateKeyHandlingMode duplicateKeyHandlingMode)
	{
		var dictionary = new OrderedMultiDictionary<string, string>(duplicateKeyHandlingMode);

		dictionary.Add("apple", "red");
		dictionary.Add("apple", "green");

		string[] expected;

		if (duplicateKeyHandlingMode == DuplicateKeyHandlingMode.PreventDuplicateKeys)
			expected = ["green"];
		else
			expected = ["red", "green"];

		Assert.Multiple(() => {
			Assert.That(dictionary.Keys, Has.Count.EqualTo(1)); // Explicitly test Count property
			Assert.That(dictionary.Keys, Is.EquivalentTo(new[] { "apple" }));
			Assert.That(dictionary.Values, Has.Count.EqualTo(expected.Length)); // Explicitly test Count property
			Assert.That(dictionary.Values, Is.EqualTo(expected));
		});
	}

	#endregion

	#region Set

	[Test]
	public void TestSetDifferentKeys()
	{
		var dictionary = new OrderedMultiDictionary<string, int>();

		dictionary.Add("apples", 3);
		dictionary.Add("bananas", 5);
		dictionary.Set("bananas", 8);

		Assert.Multiple(() => {
			Assert.That(dictionary.Keys, Has.Count.EqualTo(2));
			Assert.That(dictionary.Keys, Is.EquivalentTo(new[] { "apples", "bananas" }));
			Assert.That(dictionary.Values, Is.EqualTo(new[] { 3, 8 }));
		});
	}

	[TestCase(DuplicateKeyHandlingMode.LowestIndexTakesPriority)]
	[TestCase(DuplicateKeyHandlingMode.HighestIndexTakesPriority)]
	public void TestSetSameKey(DuplicateKeyHandlingMode duplicateKeyHandlingMode)
	{
		var dictionary = new OrderedMultiDictionary<string, string>(duplicateKeyHandlingMode);

		dictionary.Add("apple", "red");
		dictionary.Add("banana", "yellow");
		dictionary.Add("apple", "green");
		dictionary.Set("apple", "both");

		Assert.Multiple(() => {
			Assert.That(dictionary.Keys, Has.Count.EqualTo(2));
			Assert.That(dictionary.Keys, Is.EquivalentTo(new[] { "apple", "banana" }));
			Assert.That(dictionary.Values, Has.Count.EqualTo(2));

			if (duplicateKeyHandlingMode == DuplicateKeyHandlingMode.HighestIndexTakesPriority)
				Assert.That(dictionary.Values, Is.EqualTo(new[] { "yellow", "both" }));
			else
				Assert.That(dictionary.Values, Is.EqualTo(new[] { "both", "yellow" }));
		});
	}

	[TestCase(DuplicateKeyHandlingMode.LowestIndexTakesPriority)]
	[TestCase(DuplicateKeyHandlingMode.HighestIndexTakesPriority)]
	public void TestSetPreservesOrder(DuplicateKeyHandlingMode duplicateKeyHandlingMode)
	{
		var dictionary = new OrderedMultiDictionary<string, string>(duplicateKeyHandlingMode);

		dictionary.Add("apple", "red");
		dictionary.Add("pepper", "yellow");
		dictionary.Add("banana", "yellow");
		dictionary.Add("pepper", "green");
		dictionary.Add("apple", "green");
		dictionary.Set("pepper", "red");
		dictionary["grape"] = "purple"; // Set method and set indexer should behave identically

		// After this, dictionary should have:
		// - apple  => red
		// - pepper => red      (this and the next one should be swapped for Highest priority mode)
		// - banana => yellow
		// - apple  => green
		// - grape  => purple

		KeyValuePair<string, string>[] expected;

		if (duplicateKeyHandlingMode == DuplicateKeyHandlingMode.HighestIndexTakesPriority)
		{
			expected = [
				new KeyValuePair<string, string>("apple", "red"),
				new KeyValuePair<string, string>("banana", "yellow"),
				new KeyValuePair<string, string>("pepper", "red"),
				new KeyValuePair<string, string>("apple", "green"),
				new KeyValuePair<string, string>("grape", "purple"),
			];
		}
		else
		{
			expected = [
				new KeyValuePair<string, string>("apple", "red"),
				new KeyValuePair<string, string>("pepper", "red"),
				new KeyValuePair<string, string>("banana", "yellow"),
				new KeyValuePair<string, string>("apple", "green"),
				new KeyValuePair<string, string>("grape", "purple"),
			];
		}

		Assert.Multiple(() => {
			Assert.That(dictionary.Values, Has.Count.EqualTo(expected.Length)); // Explicitly test Count property
			Assert.That(dictionary, Is.EqualTo(expected));
		});
	}

	#endregion

	#region Remove

	[Test]
	public void TestAddAndRemoveAll()
	{
		var dictionary = new OrderedMultiDictionary<string, string>();

		dictionary.Add("apple", "red");
		dictionary.Add("apple", "green");
		dictionary.RemoveAll("apple");

		Assert.Multiple(() => {
			Assert.That(dictionary.Keys, Is.EqualTo(Array.Empty<string>()));
			Assert.That(dictionary.Values, Is.EqualTo(Array.Empty<string>()));
		});
	}

	[Test]
	public void TestAddAndRemoveNonExistent()
	{
		var dictionary = new OrderedMultiDictionary<string, int>();

		dictionary.Add("apples", 3);
		dictionary.RemoveAll("bananas");

		Assert.Multiple(() => {
			Assert.That(dictionary.Keys, Is.EqualTo(new[] { "apples" }));
			Assert.That(dictionary.Values, Is.EqualTo(new[] { 3 }));
		});
	}

	#endregion

	#region Clear

	[Test]
	public void TestClear()
	{
		var dictionary = new OrderedMultiDictionary<string, int>();

		dictionary.Add("apples", 3);
		dictionary.Add("bananas", 5);
		dictionary.Clear();

		Assert.Multiple(() => {
			Assert.That(dictionary, Has.Count.EqualTo(0)); // Explicitly test the "Count" property
			Assert.That(dictionary.Keys, Is.Empty);
			Assert.That(dictionary.Values, Is.Empty);
		});
	}

	#endregion

	#region Contains

	[Test]
	public void TestContainsAfterSimpleAdd()
	{
		var dictionary = new OrderedMultiDictionary<string, int>();

		Assert.That(dictionary.ContainsKey("apples"), Is.False);

		dictionary.Add("apples", 3);

		Assert.That(dictionary.ContainsKey("apples"));
	}

	[Test]
	public void TestContainsAfterAddAndRemove()
	{
		var dictionary = new OrderedMultiDictionary<string, int>();

		dictionary.Add("apples", 3);
		dictionary.RemoveAll("apples");

		Assert.That(dictionary.ContainsKey("apples"), Is.False);
	}

	#endregion

	#region Get

	[TestCase(DuplicateKeyHandlingMode.LowestIndexTakesPriority)]
	[TestCase(DuplicateKeyHandlingMode.HighestIndexTakesPriority)]
	public void TestGetSingle(DuplicateKeyHandlingMode duplicateKeyHandlingMode)
	{
		var dictionary = new OrderedMultiDictionary<string, string>(duplicateKeyHandlingMode);

		dictionary.Add("apple", "red");
		dictionary.Add("pepper", "yellow");
		dictionary.Add("pepper", "green");
		dictionary.Add("apple", "green");
		dictionary.Set("pepper", "red");
		dictionary.Set("grape", "purple");

		// After this, dictionary should have:
		// - apple  => red
		// - pepper => red
		// - apple  => green
		// - grape  => purple

		Assert.Multiple(() => {
			if (duplicateKeyHandlingMode == DuplicateKeyHandlingMode.HighestIndexTakesPriority)
				Assert.That(dictionary["apple"], Is.EqualTo("green"), "Single-value access by key should return highest-index value");
			else
				Assert.That(dictionary["apple"], Is.EqualTo("red"), "Single-value access by key should return lowest-index value");

			Assert.That(dictionary["pepper"], Is.EqualTo("red"));
			Assert.That(dictionary["grape"], Is.EqualTo("purple"));
			Assert.Throws<KeyNotFoundException>(() => _ = dictionary["pear"], "Single-value access for non-existent key should throw");
		});
	}

	[Test]
	public void TestGetMultiple()
	{
		var dictionary = new OrderedMultiDictionary<string, string>();

		dictionary.Add("apple", "red");
		dictionary.Add("pepper", "yellow");
		dictionary.Add("pepper", "green");
		dictionary.Add("apple", "green");
		dictionary.Set("pepper", "red");
		dictionary.Set("grape", "purple");

		// After this, dictionary should have:
		// - apple  => red
		// - pepper => red
		// - apple  => green
		// - grape  => purple

		Assert.Multiple(() => {
			Assert.That(dictionary.GetAllValues("apple"), Is.EqualTo(new[] { "red", "green" }));
			Assert.That(dictionary.GetAllValues("pepper"), Is.EqualTo(new[] { "red" }));
			Assert.That(dictionary.GetAllValues("grape"), Is.EqualTo(new[] { "purple" }));
			Assert.That(dictionary.GetAllValues("pear"), Is.EqualTo(Array.Empty<string>()));
		});
	}

	#endregion

	#region IDictionary<TKey, TValue> Contract

	[Test]
	public void TestDictionaryReadOnlyCollections()
	{
		var orderedDictionary = new OrderedMultiDictionary<string, int>();

		orderedDictionary.Add("apples", 3);
		orderedDictionary.Add("bananas", 5);

		IDictionary<string, int> dictionary = orderedDictionary;

		Assert.Multiple(() => {
			TestCollection(dictionary.Keys, "apples", "pears");
			TestCollection(dictionary.Values, 5, -1);
			TestCollection(dictionary, new KeyValuePair<string, int>("apples", 3), new KeyValuePair<string, int>("pears", -1));
		});

#pragma warning disable NUnit2045
		static void TestCollection<T>(ICollection<T> collection, T itemInCollection, T itemNotInCollection)
		{
			Assert.That(collection.Contains(itemInCollection), Is.True);
			Assert.That(collection.Contains(itemNotInCollection), Is.False);

			if (collection.IsReadOnly)
			{
				Assert.Throws<NotSupportedException>(() => collection.Add(default!));
				Assert.Throws<NotSupportedException>(() => collection.Remove(default!));
				Assert.Throws<NotSupportedException>(collection.Clear);
			}

			TestCollectionCopyTo(collection);
		}

		static void TestCollectionCopyTo<T>(ICollection<T> collection)
		{
			Assert.Throws<ArgumentNullException>(() => collection.CopyTo(null!, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => collection.CopyTo([], 1));

			var tooSmallArray = new T[collection.Count - 1];

			Assert.Throws<ArgumentException>(() => collection.CopyTo(tooSmallArray, 0));

			var equalSizeArray = new T[collection.Count];

			Assert.Throws<ArgumentException>(() => collection.CopyTo(equalSizeArray, 1));

			collection.CopyTo(equalSizeArray, 0);

			Assert.That(equalSizeArray, Is.EqualTo(collection));
		}
#pragma warning restore NUnit2045
	}

	[Test]
	public void TestDictionaryAsIDictionary()
	{
		var orderedDictionary = new OrderedMultiDictionary<string, int>();

		orderedDictionary.Add("apples", 3);
		orderedDictionary.Add("apples", 5);

		IDictionary<string, int> dictionary = orderedDictionary;

		dictionary.Remove("apples");

		// Should not contain ANY "apples" items now
		Assert.That(dictionary.ContainsKey("apples"), Is.False);
	}

	[Test]
	public void TestDictionaryAsCollection()
	{
		var orderedDictionary = new OrderedMultiDictionary<string, int>();

		orderedDictionary.Add("apples", 3);
		orderedDictionary.Add("bananas", 5);

		ICollection<KeyValuePair<string, int>> dictionaryAsCollection = orderedDictionary;

		Assert.Multiple(() => {
			dictionaryAsCollection.Add(new KeyValuePair<string, int>("pears", 10));
			dictionaryAsCollection.Remove(new KeyValuePair<string, int>("pears", 5));

			// Should still have { "pears", 10 }
			Assert.That(dictionaryAsCollection.Contains(new KeyValuePair<string, int>("pears", 10)));

			dictionaryAsCollection.Remove(new KeyValuePair<string, int>("pears", 10));

			Assert.That(dictionaryAsCollection.Contains(new KeyValuePair<string, int>("pears", 10)), Is.False);
		});
	}

	#endregion
}