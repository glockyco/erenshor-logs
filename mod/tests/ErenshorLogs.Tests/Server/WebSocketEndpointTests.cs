using ErenshorLogs.Server;
using Xunit;

namespace ErenshorLogs.Tests.Server;

public sealed class WebSocketEndpointTests
{
  [Theory]
  [InlineData(false, "ws://127.0.0.1:38729")]
  [InlineData(true, "ws://0.0.0.0:38729")]
  public void CreateBindLocation_UsesLoopbackUnlessLanIsEnabled(
    bool allowLanConnections,
    string expected
  )
  {
    Assert.Equal(expected, WebSocketEndpoint.CreateBindLocation(38729, allowLanConnections));
  }
}
