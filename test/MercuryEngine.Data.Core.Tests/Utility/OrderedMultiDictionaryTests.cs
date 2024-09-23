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

	[Test]
	public void TestAddMultipleSameKey()
	{
		var dictionary = new OrderedMultiDictionary<string, string>();

		dictionary.Add("apple", "red");
		dictionary.Add("apple", "green");

		Assert.Multiple(() => {
			Assert.That(dictionary.Keys, Has.Count.EqualTo(1)); // Explicitly test Count property
			Assert.That(dictionary.Keys, Is.EquivalentTo(new[] { "apple" }));
			Assert.That(dictionary.Values, Has.Count.EqualTo(2)); // Explicitly test Count property
			Assert.That(dictionary.Values, Is.EqualTo(new[] { "red", "green" }));
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

	[Test]
	public void TestSetSameKey()
	{
		var dictionary = new OrderedMultiDictionary<string, string>();

		dictionary.Add("apple", "red");
		dictionary.Add("banana", "yellow");
		dictionary.Add("apple", "green");
		dictionary.Set("apple", "both");

		Assert.Multiple(() => {
			Assert.That(dictionary.Keys, Has.Count.EqualTo(2));
			Assert.That(dictionary.Keys, Is.EquivalentTo(new[] { "apple", "banana" }));
			Assert.That(dictionary.Values, Has.Count.EqualTo(2));
			Assert.That(dictionary.Values, Is.EqualTo(new[] { "yellow", "both" }));
		});
	}

	[Test]
	public void TestSetPreservesOrder()
	{
		var dictionary = new OrderedMultiDictionary<string, string>();

		dictionary.Add("apple", "red");
		dictionary.Add("pepper", "yellow");
		dictionary.Add("pepper", "green");
		dictionary.Add("apple", "green");
		dictionary.Set("pepper", "red");
		dictionary["grape"] = "purple"; // Set method and set indexer should behave identically

		// After this, dictionary should have:
		// - apple  => red
		// - pepper => red
		// - apple  => green
		// - grape  => purple

		Assert.Multiple(() => {
			Assert.That(dictionary.Values, Has.Count.EqualTo(4));
			Assert.That(dictionary, Is.EqualTo(new[] {
				new KeyValuePair<string, string>("apple", "red"),
				new KeyValuePair<string, string>("pepper", "red"),
				new KeyValuePair<string, string>("apple", "green"),
				new KeyValuePair<string, string>("grape", "purple"),
			}));
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

	[Test]
	public void TestGetSingle()
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
			Assert.That(dictionary["apple"], Is.EqualTo("green"), "Single-value access by key should return highest-index value");
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
}