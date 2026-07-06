using UnityEngine;

public interface IToolTippable
{
    string name { get;  }
    string description { get; }
    Sprite icon { get; }
}