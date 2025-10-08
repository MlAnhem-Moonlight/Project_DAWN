using System.Collections;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public string currentAnimation = "";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeAnimation(Animator animator, string animation, float crossFade = 0.1f, float time = 0)
    {
        if (time > 0) StartCoroutine(Wait());
        else Validate();

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(time - crossFade);
            Validate();
        }

        void Validate()
        {
            if (currentAnimation != animation)
            {
                currentAnimation = animation;
                animator.CrossFade(animation, crossFade);
            }
            //Debug.Log($"Change animation to {animation} (current={currentAnimation})" );
        }
    }

}
