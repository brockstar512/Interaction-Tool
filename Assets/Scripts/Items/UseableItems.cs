using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//todo each scriptable iobject needs to be in a file with the name being the same as the class name

public abstract class ItemFactory : ScriptableObject
{
    public abstract IItem CreateItem();

}
[CreateAssetMenu(fileName = "Key", menuName = "ItemFactory/Key")]
public abstract class KeyFactory : ItemFactory
{
    public override IItem CreateItem()
    {
        return new Key();
    }

}

public abstract class SwordFactory : ItemFactory
{
    public override IItem CreateItem()
    {
        return new Sword();
    }

}
public abstract class WhipFactory : ItemFactory
{
    public override IItem CreateItem()
    {
        return new Whip();
    }

}
public abstract class CandleFactory : ItemFactory
{
    public override IItem CreateItem()
    {
        return new Candle();
    }

}

public abstract class PlankFactory : ItemFactory
{
    public override IItem CreateItem()
    {
        return new Plank();
    }

}

public abstract class GrapplingHookFactory : ItemFactory
{
    public override IItem CreateItem()
    {
        return new GrapplingHook();
    }

}

public abstract class BellFactory : ItemFactory
{
    public override IItem CreateItem()
    {
        return new Bell();
    }

}


public class Key : IItem
{
    public Sprite Sprite { get; }

    public void Use()
    {
        
    }

}

public class Whip : IItem
{
    public Sprite Sprite { get; }

    public void Use()
    {
        
    }
}

public class Sword : IItem
{
    public Sprite Sprite { get; }

    public void Use()
    {
        
    }
}

public class Candle : IItem
{
    public Sprite Sprite { get; }

    public void Use()
    {
        
    }
}

public class Plank : IItem
{
    public Sprite Sprite { get; }

    public void Use()
    {
        
    }   
}
public class GrapplingHook : IItem
{
    public Sprite Sprite { get; }

    public void Use()
    {
        
    }
}

public class Bell : IItem
{
    public Sprite Sprite { get; }

    public void Use()
    {
        
    }
}

