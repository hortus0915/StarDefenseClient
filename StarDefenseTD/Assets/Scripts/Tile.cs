using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
   public bool IsBuuldTower {get; set;}
   

   private void Awake()
   {
       IsBuuldTower = false;
   }

   
}
