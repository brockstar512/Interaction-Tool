using UnityEngine;

public class PullableOpenDependent : MonoBehaviour, IPullDependent
{
    private PullableDoorAnimation _doubleDoorAnimation;

   
    private void Awake()
    {
        _doubleDoorAnimation = new PullableDoorAnimation(GetComponent<Animator>());
    }

    public void OnPullChanged(float amount) => _doubleDoorAnimation.Step(amount);
    


}
