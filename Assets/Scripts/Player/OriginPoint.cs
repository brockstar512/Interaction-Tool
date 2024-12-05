using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class OriginPoint : MonoBehaviour
{
   public SpriteRenderer getSpriteRenderer => _sr;
   public Sprite getSprite => _sr.sprite;
   private SpriteRenderer _sr;

   private void Awake()
   {
      _sr = GetComponent<SpriteRenderer>();
      _sr.sprite = null;
   }
}
