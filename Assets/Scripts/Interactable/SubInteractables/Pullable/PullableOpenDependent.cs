using UnityEngine;

public class PullableOpenDependent : MonoBehaviour, IDependent<float>
{
    private PullableDoorAnimation _doubleDoorAnimation;

   
    private void Awake()
    {
        _doubleDoorAnimation = new PullableDoorAnimation(GetComponent<Animator>());
    }

    public void UpdateDependentValue(float amount) => _doubleDoorAnimation.Step(amount);
    


}
