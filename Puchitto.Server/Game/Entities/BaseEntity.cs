using Puchitto.Server.Clients;
using Puchitto.Server.Game.Entities.Scripting;
using Puchitto.Server.Management;
using Puchitto.Server.Packets.Engine.Bidirectional;
using Puchitto.Server.Packets.Serialization.Facades;
using Puchitto.Server.Realms;
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
    public int Id { get; set; }
    
    /// <summary>
    /// Is this object visible?
    /// </summary>
    public bool Visible { get; set; }
    
    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// The tag this entity has.
    /// </summary>
    public string? Tag { get; set; }
    
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
    /// The realm that owns this entity.
    /// </summary>
    public Realm OwningRealm { get; private set; } = null!;
    
    /// <summary>
    /// The Puchitto systems provider.
    /// </summary>
    public IPuchittoSystemsProvider SystemsProvider => OwningRealm.SystemsProvider;

    /// <summary>
    /// The MiniAntics environment.
    /// </summary>
    private MiniAnticsEnvironment _environment = null!;

    /// <summary>
    /// The antics for this entity.
    /// </summary>
    private List<ObjectAntics> _antics = new();

    /// <summary>
    /// Locks the environment while an RPC runs.
    /// </summary>
    private Lock _rpcLock = new();
    
    public void Initialize(Realm realm)
    {
        OwningRealm = realm;
        _environment = SystemsProvider.MakeChildEnvironment();
    }
    
    /// <summary>
    /// Initializes an entity.
    /// </summary>
    /// <param name="realm">The realm that owns this entity.</param>
    /// <param name="entityData">The entity data.</param>
    public void Initialize(
        Realm realm,
        LevelEntityData entityData)
    {
        Initialize(realm);

        Id = entityData.Id;
        IsAuthored = true;
        
        Name = entityData.Name;
        Visible = entityData.Visible;
        Tag = entityData.Tag;
        
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
                Script = new MiniAnticsScript(a.Script),
                Name = a.Name
            }).ToList() ?? [];

        Transform.Position = entityData.Transform.Position;
        Transform.Rotation = entityData.Transform.Rotation;
        Transform.Scale = entityData.Transform.Scale;
    }

    /// <summary>
    /// Called when this object is attached.
    /// </summary>
    public virtual void OnAttached()
    {
        SetupCustomMiniAnticsEnvironment(_environment);
        RunAntics(AnticsOn.Attach);
    }
    
    /// <summary>
    /// Sets up the custom miniantics environment for this entity.
    /// </summary>
    /// <param name="environment">The environment.</param>
    protected virtual void SetupCustomMiniAnticsEnvironment(MiniAnticsEnvironment environment)
    {
        environment.Set("invoke-rpc", (string name, Client client) =>
        {
            var miniAnticsRpcPacket = new MiniAnticsRpcPacket
            {
                Name = name,
                ObjectId = Id
            };
            var writer = client.BeginDataSend(miniAnticsRpcPacket.PacketId);
            miniAnticsRpcPacket.Serialize(writer);

            return writer;
        });
        
        environment.Set("send", (WriterFacade f) =>
        {
            _ = Task.Run(async () =>
            {
                await f.Client.FinishDataSend(f);
            });
        });
        
        environment.Set("visible", () => Visible);
        environment.Set("set-visible", (bool visible) => Visible = visible);
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
    /// Handles an RPC sent by a client.
    /// </summary>
    /// <param name="name">The name of the RPC.</param>
    /// <param name="sender">The sender client.</param>
    public void HandleRpc(string name, Client sender)
    {
        // NOTE: We lock here because we don't want multiple clients calling an RPC overwriting the sender field.
        //       Is this really the best solution?
        lock (_rpcLock)
        {
            _environment.Set("sender", sender);
            RunAntics(AnticsOn.Rpc, name);
            _environment.Unset("sender");
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