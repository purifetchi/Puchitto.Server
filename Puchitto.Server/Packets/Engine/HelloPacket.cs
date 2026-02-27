using Puchitto.Server.Packets.Serialization;

namespace Puchitto.Server.Packets.Engine;

/// <summary>
/// A Puchitto hello packet.
/// </summary>
public struct HelloPacket(string branding, string gameRulesName) : IPuchittoPacket
{
    /// <inheritdoc />
    public int PacketId { get; } = 1;

    /// <summary>
    /// The identification message.
    /// </summary>
    private string Branding { get; set; } = branding;

    /// <summary>
    /// The game rules name.
    /// </summary>
    private string GameRulesName { get; set; } = gameRulesName;
    
    /// <inheritdoc />
    public void Serialize(ref NetworkWriter writer)
    {
        writer.WriteString(Branding);
        writer.WriteString(GameRulesName);
    }

    /// <inheritdoc />
    public void Deserialize(ref NetworkReader reader)
    {
        Branding = reader.ReadString();
        GameRulesName = reader.ReadString();
    }
}