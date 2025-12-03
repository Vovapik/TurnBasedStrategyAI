using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Transform target;     
    public SpriteRenderer fill; 
    public SpriteRenderer background;

    private int maxHp;

    public void Init(Transform follow, int maxHealth)
    {
        target = follow;
        this.maxHp = maxHealth;

        float tileSize = FindObjectOfType<MapGenerator>().GetTileWorldSize();

        float barWidth  = tileSize * 0.9f;   
        float barHeight = tileSize * 0.5f; 

        background.size = new Vector2(barWidth, barHeight);
        fill.size       = new Vector2(barWidth, barHeight);

       
        SpriteRenderer sr = follow.GetComponentInChildren<SpriteRenderer>();
        float unitHeight = sr.bounds.size.y;

        transform.position = follow.position + new Vector3(0, unitHeight * 0.6f, 0);
    }


    public void SetHealth(int hp)
    {
        float ratio = (float)hp / maxHp;
        float width = background.size.x * ratio;

        fill.size = new Vector2(width, background.size.y);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        SpriteRenderer sr = target.GetComponentInChildren<SpriteRenderer>();
        float unitHeight = sr.bounds.size.y;

        transform.position = target.position + new Vector3(0, unitHeight * 0.6f, 0);
    }
}