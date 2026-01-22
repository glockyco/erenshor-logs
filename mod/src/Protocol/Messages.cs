using ErenshorLogs.Events;

namespace ErenshorLogs.Protocol;

/// <summary>
/// Message sent to clients immediately upon WebSocket connection.
/// Contains protocol version, mod version, and current session (if any).
/// </summary>
public record HandshakeMessage(
  string Type,
  string ProtocolVersion,
  string ModVersion,
  SessionInfo? Session
)
{
  /// <summary>
  /// Creates a handshake message.
  /// </summary>
  public static HandshakeMessage Create(string modVersion, SessionInfo? session) =>
    new(
      Type: "handshake",
      ProtocolVersion: Protocol.ProtocolVersion.Current,
      ModVersion: modVersion,
      Session: session
    );
}

/// <summary>
/// Notification sent when a combat session starts.
/// </summary>
public record SessionStartMessage(string Type, SessionInfo Session)
{
  /// <summary>
  /// Creates a session start message.
  /// </summary>
  public static SessionStartMessage Create(SessionInfo session) =>
    new(Type: "sessionStart", Session: session);
}

/// <summary>
/// Notification sent when a combat session ends.
/// </summary>
public record SessionEndMessage(string Type, string SessionId, long EndTime)
{
  /// <summary>
  /// Creates a session end message.
  /// </summary>
  public static SessionEndMessage Create(string sessionId, long endTime) =>
    new(Type: "sessionEnd", SessionId: sessionId, EndTime: endTime);
}

/// <summary>
/// Batched combat events sent periodically to clients.
/// </summary>
public record CombatEventsMessage(string Type, string SessionId, CombatEvent[] Events)
{
  /// <summary>
  /// Creates a combat events message.
  /// </summary>
  public static CombatEventsMessage Create(string sessionId, CombatEvent[] events) =>
    new(Type: "combatEvents", SessionId: sessionId, Events: events);
}

/// <summary>
/// Session information included in handshake and session start messages.
/// </summary>
public record SessionInfo(string Id, long StartTime);
