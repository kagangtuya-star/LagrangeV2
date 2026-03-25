using System.Buffers;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization;

namespace Lagrange.Core.Test.Service;

[Parallelizable]
public class FetchGroupsResponseTest
{
    [Test]
    public void TestFetchGroupsResponse_IgnoresMalformedCustomInfoPayload()
    {
        byte[] infoPayload = WritePayload(writer =>
        {
            writer.EncodeVarInt(42u);
            writer.EncodeString("test-group");
        });

        byte[] groupPayload = WritePayload(writer =>
        {
            writer.EncodeVarInt(24u);
            writer.EncodeVarInt(123456789L);

            writer.EncodeVarInt(34u);
            writer.EncodeBytes(infoPayload);

            writer.EncodeVarInt(42u);
            writer.EncodeBytes([0x08, 0x80]);
        });

        byte[] responsePayload = WritePayload(writer =>
        {
            writer.EncodeVarInt(18u);
            writer.EncodeBytes(groupPayload);
        });

        var response = ProtoSerializer.DeserializeProtoPackable<FetchGroupsResponse>(responsePayload);

        Assert.That(response.Groups, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(response.Groups[0].GroupUin, Is.EqualTo(123456789L));
            Assert.That(response.Groups[0].Info.GroupName, Is.EqualTo("test-group"));
        });
    }

    private static byte[] WritePayload(Action<ProtoWriter> write)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new ProtoWriter(buffer);
        write(writer);
        writer.Flush();
        return buffer.WrittenMemory.ToArray();
    }
}
