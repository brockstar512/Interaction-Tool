using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItem
{
    public Sprite Sprite { get; }

    public void Use(Vector3 playerDirection);

}
