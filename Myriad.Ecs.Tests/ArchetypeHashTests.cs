using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds.Archetypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests;

[TestClass]
public class ArchetypeHashTests
{
    [TestMethod]
    public void ArchetypeHashEqual()
    {
        var hash1 = new ArchetypeHash()
           .Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash1);

        var hash2 = new ArchetypeHash()
           .Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash1);

        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void ArchetypeHashNotEqual()
    {
        var hash1 = new ArchetypeHash();
        hash1 = hash1.Toggle(ComponentId.Get<ComponentInt16>());
        Console.WriteLine(hash1);

        var hash2 = new ArchetypeHash();
        hash2 = hash2.Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash2);

        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void ArchetypeHashRemoveComponents()
    {
        var hash1 = new ArchetypeHash()
           .Toggle(ComponentId.Get<ComponentInt16>())
            .Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash1);

        // Create the same hash again, with one extra item
        var hash2 = new ArchetypeHash()
           .Toggle(ComponentId.Get<ComponentInt16>())
           .Toggle(ComponentId.Get<ComponentFloat>())
           .Toggle(ComponentId.Get<ComponentInt32>());
        Console.WriteLine(hash2);
        Assert.AreNotEqual(hash1, hash2);

        // Remove the extra item
        hash2 = hash2.Toggle(ComponentId.Get<ComponentInt32>());
        Assert.AreEqual(hash1, hash2);
    }
}
