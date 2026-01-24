using ErenshorLogs.Server;
using Xunit;

namespace ErenshorLogs.Tests.Server;

public class NetworkUtilsTests
{
  [Fact]
  public void GetLocalIPv4Addresses_ReturnsListOfAddresses()
  {
    // Act
    var addresses = NetworkUtils.GetLocalIPv4Addresses();

    // Assert
    Assert.NotNull(addresses);
    // Can't assert specific IPs since test environment varies
    // Just verify it returns a list (may be empty in some CI environments)
  }

  [Fact]
  public void GetLocalIPv4Addresses_ExcludesLoopback()
  {
    // Act
    var addresses = NetworkUtils.GetLocalIPv4Addresses();

    // Assert
    Assert.DoesNotContain("127.0.0.1", addresses);
  }

  [Fact]
  public void GetLocalIPv4Addresses_SortsPreferringHomeNetworkRanges()
  {
    // Act
    var addresses = NetworkUtils.GetLocalIPv4Addresses();

    // Assert
    if (addresses.Count > 1)
    {
      // If we have multiple addresses, check that 192.168.x.x comes before other ranges
      var first192 = addresses.FindIndex(ip => ip.StartsWith("192.168."));
      var first10 = addresses.FindIndex(ip => ip.StartsWith("10."));

      if (first192 >= 0 && first10 >= 0)
      {
        Assert.True(
          first192 < first10,
          "192.168.x.x addresses should be sorted before 10.x.x.x addresses"
        );
      }
    }
  }

  [Fact]
  public void GetInterfaceName_ReturnsNameForValidIP()
  {
    // Arrange
    var addresses = NetworkUtils.GetLocalIPv4Addresses();

    if (addresses.Count > 0)
    {
      var testIp = addresses[0];

      // Act
      var name = NetworkUtils.GetInterfaceName(testIp);

      // Assert
      Assert.NotNull(name);
      Assert.NotEmpty(name);
    }
  }

  [Fact]
  public void GetInterfaceName_ReturnsNullForInvalidIP()
  {
    // Act
    var name = NetworkUtils.GetInterfaceName("999.999.999.999");

    // Assert
    Assert.Null(name);
  }
}
