using System;

/// <summary>
/// Holds a player value from 0-3, which team the player belongs to, and if this player joined.
/// @Author Michael Frye
/// </summary>
[Serializable]
public class PlayerData
{
    public char playerValue = '0';
    public bool InTeam1;
    public bool joined;
    public int score;
}
