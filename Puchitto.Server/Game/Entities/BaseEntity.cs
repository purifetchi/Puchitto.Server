using Puchitto.Server.Clients;
using Puchitto.Server.Game.Entities.Scripting;
using Puchitto.Server.Management;
using Puchitto.Server.Realms.Definitions;
using Puchitto.Server.Scripting;

namespace Puchitto.Server.Game.Entities;

/// <summary>
/// The base class for an entity.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// The type of the entity.
    /// </summary>
    public abstract string Type { get; }

    /// <summary>
    /// The ID of the entity.
    /// </summary>
    public required int Id { get; set; }
    
    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Is the current entity authored by the level?
    /// </summary>
    public bool IsAuthored { get; set; }

    /// <summary>
    /// The transform of the entity.
    /// </summary>
    public Transform Transform { get; } = new();

    /// <summary>
    /// The owning client.
    /// </summary>
    public Client? Owner { get; set; }

    /// <summary>
    /// The MiniAntics environment.
    /// </summary>
    private readonly MiniAnticsEnvironment _environment;

    /// <summary>
    /// The antics for this entity.
    /// </summary>
    private readonly List<ObjectAntics> _antics = new();

    /// <summary>
    /// The base constructor for an entity.
    /// </summary>
    /// <param name="puchittoSystemsProvider">
    /// The Puchitto systems provider.
    /// </param>
    public BaseEntity(IPuchittoSystemsProvider puchittoSystemsProvider)
    {
        _environment = puchittoSystemsProvider.MakeChildEnvironment();
    }
    
    /// <summary>
    /// The base constructor for an entity.
    /// </summary>
    /// <param name="puchittoSystemsProvider">
    /// The Puchitto systems provider.
    /// </param>
    public BaseEntity(
        IPuchittoSystemsProvider puchittoSystemsProvider,
        LevelEntityData entityData)
    {
        _environment = puchittoSystemsProvider.MakeChildEnvironment();

        IsAuthored = true;
        
        Name = entityData.Name;
        _antics = entityData.Antics?
            .Select(a => new ObjectAntics
            {
                On = a.On switch
                {
                    "attach" => AnticsOn.Attach,
                    "click" => AnticsOn.Click,
                    "rpc" => AnticsOn.Rpc,
                    _ => throw new NotImplementedException()
                },
                Script = new MiniAnticsScript(a.Script)
            }).ToList() ?? [];

        Transform.Position = entityData.Transform.Position;
        Transform.Rotation = entityData.Transform.Rotation;
        Transform.Scale = entityData.Transform.Scale;
    }
    
    /// <summary>
    /// Runs the antics for this object.
    /// </summary>
    /// <param name="on">What antics attach point should be used?</param>
    /// <param name="name">Sets the specified name we're interested in.</param>
    public void RunAntics(AnticsOn on, string? name = null)
    {
        var isNameEmpty = string.IsNullOrEmpty(name);
        foreach (var antic in _antics)
        {
            if (antic.On != on)
            {
                continue;
            }

            if (!isNameEmpty && antic.Name != name)
            {
                continue;
            }

            try
            {
                antic.Script.Run(_environment);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to run antics: {0}", e.Message);
            }
        }
    }
    
    /// <summary>
    /// Gets the entity data for serialization.
    /// </summary>
    /// <returns>
    /// The entity data.
    /// </returns>
    public object GetEntityDataForSerialization()
    {
        // TODO
        return new object();
    }
}