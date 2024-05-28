using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface ICommand 
{
    public Task Execute();
    public bool CanExecute();
    /*
         public async Task Execute()
    {
        //Play();

       
    }*/
}
