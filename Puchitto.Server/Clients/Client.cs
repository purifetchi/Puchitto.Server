using System.Buffers;
using Puchitto.Server.Packets;
using Puchitto.Server.Packets.Serialization;
using Puchitto.Server.Packets.Serialization.Facades;
using Puchitto.Server.Realms;

namespace Puchitto.Server.Clients;

/// <summary>
/// A Puchitto client.
/// </summary>
public class Client
{
    /// <summary>
    /// The ID of this client.
    /// </summary>
    public Guid Id { get; }
    
    /// <summary>
    /// The connection for this client.
    /// </summary>
    public ClientConnection Connection { get; }
    
    /// <summary>
    /// The current realm the player is in.
    /// </summary>
    public Realm CurrentRealm { get; set; }

    /// <summary>
    /// The client state.
    /// </summary>
    public ClientState State => _state;
    
    /// <summary>
    /// The last sent sequence id.
    /// </summary>
    private int _lastSentSequenceId;

    /// <summary>
    /// The client state.
    /// </summary>
    private ClientState _state = ClientState.Established;
    
    public Client(
        Guid id,
        ClientConnection connection)
    {
        Id = id;
        Connection = connection;
    }

    /// <summary>
    /// Sends a packet to this client.
    /// </summary>
    /// <param name="data">
    /// The packet data.
    /// </param>
    /// <typeparam name="TPacket">
    /// The type of the packet.
    /// </typeparam>
    public async Task SendData<TPacket>(TPacket data)
        where TPacket : struct, IPuchittoPacket
    {
        if (State == ClientState.Disconnected)
        {
            return;
        }

        var buffer = RentBuffer();
        
        var writer = new NetworkWriter(buffer, 0);
        WriteEnvelope(data.PacketId, ref writer);
        data.Serialize(ref writer);

        await AdjustPacketLengthAndSend(
            writer.Position,
            buffer);

        ReturnBuffer(buffer);
    }

    /// <summary>
    /// Begins sending data.
    /// </summary>
    /// <returns>
    /// The writer facade.
    /// </returns>
    public WriterFacade BeginDataSend(int opCode)
    {
        // TODO: The facade should be pooled.
        var buffer = RentBuffer();
        var writer = new NetworkWriter(buffer, 0);
        WriteEnvelope(opCode, ref writer);
        
        return new WriterFacade(buffer, this, writer.Position);
    }

    /// <summary>
    /// Finishes sending the data.
    /// </summary>
    /// <param name="facade">The facade.</param>
    public async Task FinishDataSend(WriterFacade facade)
    {
        await AdjustPacketLengthAndSend(
            facade.Written,
            facade.BackingArray);
        
        ReturnBuffer(facade.BackingArray);
    }

    /// <summary>
    /// Rents a buffer.
    /// </summary>
    /// <returns>
    /// The buffer.
    /// </returns>
    private byte[] RentBuffer()
    {
        // 10KiB by default.
        const int bufferSizeInBytes = 1024 * 10;
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSizeInBytes);

        return buffer;
    }

    /// <summary>
    /// Writes the envelope.
    /// </summary>
    /// <param name="opCode">The packet's opcode.</param>
    /// <param name="writer">The envelope.</param>
    private void WriteEnvelope(
        int opCode,
        ref NetworkWriter writer)
    {
        var sequenceId = Interlocked.Increment(ref _lastSentSequenceId);
        var envelope = new PacketEnvelope(sequenceId, opCode, 0);
        
        envelope.Serialize(ref writer);
    }

    /// <summary>
    /// Adjusts the envelope's length and sends the data.
    /// </summary>
    /// <param name="length">
    /// The length.
    /// </param>
    /// <param name="buffer">
    /// The buffer.
    /// </param>
    private async Task AdjustPacketLengthAndSend(
        int length,
        byte[] buffer)
    {
        var actualLength = length - PacketEnvelope.EnvelopeSize;
        var nw = new NetworkWriter(buffer, PacketEnvelope.LengthOffset);
        nw.WriteInt32(actualLength);
        
        await Connection.SendBuffer(new ArraySegment<byte>(buffer, 0, length));
    }

    /// <summary>
    /// Returns the rented buffer.
    /// </summary>
    /// <param name="buffer">
    /// The buffer.
    /// </param>
    private void ReturnBuffer(byte[] buffer)
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }

    /// <summary>
    /// Sets the current client state.
    /// </summary>
    /// <param name="state">The state.</param>
    public void SetState(ClientState state)
    {
        Interlocked.Exchange(ref _state, state);
    }

    /// <summary>
    /// Called when we want to disconnect a client.
    /// </summary>
    public async Task Disconnect()
    {
        await Connection.Close();
    }
}