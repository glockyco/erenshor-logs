using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ErenshorLogs.Server;

/// <summary>
/// Network utility functions for discovering local IP addresses.
/// </summary>
public static class NetworkUtils
{
  /// <summary>
  /// Gets all active local IPv4 addresses from non-loopback network interfaces.
  /// </summary>
  /// <returns>List of IPv4 addresses, sorted with common home network ranges first.</returns>
  public static List<string> GetLocalIPv4Addresses()
  {
    var addresses = new List<string>();

    try
    {
      foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
      {
        // Skip loopback, tunnel, and down interfaces
        if (
          ni.NetworkInterfaceType == NetworkInterfaceType.Loopback
          || ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel
          || ni.OperationalStatus != OperationalStatus.Up
        )
        {
          continue;
        }

        var properties = ni.GetIPProperties();
        foreach (var ip in properties.UnicastAddresses)
        {
          // Only include IPv4 addresses
          if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
          {
            addresses.Add(ip.Address.ToString());
          }
        }
      }

      // Sort addresses: 192.168.x.x first (most common home networks), then others
      addresses = addresses
        .OrderBy(ip =>
          ip.StartsWith("192.168.") ? 0
          : ip.StartsWith("10.") ? 1
          : 2
        )
        .ThenBy(ip => ip)
        .ToList();
    }
    catch (Exception)
    {
      // Swallow network enumeration errors - return empty list
      // The caller will handle the case where no addresses are found
    }

    return addresses;
  }

  /// <summary>
  /// Gets the network interface name for a given IP address.
  /// </summary>
  /// <param name="ipAddress">The IP address to look up.</param>
  /// <returns>Network interface name, or null if not found.</returns>
  public static string? GetInterfaceName(string ipAddress)
  {
    try
    {
      foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
      {
        if (ni.OperationalStatus != OperationalStatus.Up)
        {
          continue;
        }

        var properties = ni.GetIPProperties();
        foreach (var ip in properties.UnicastAddresses)
        {
          if (ip.Address.ToString() == ipAddress)
          {
            return ni.Name;
          }
        }
      }
    }
    catch (Exception)
    {
      // Swallow errors
    }

    return null;
  }
}
