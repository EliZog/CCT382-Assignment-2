using UnityEngine;

public class HoverExpand : MonoBehaviour
{
  
    public void PointerEnter()
    {
        transform.localScale = new Vector3(1.2f, 1.2f, 1f);
    }

    
    public void PointerExit()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    
}
