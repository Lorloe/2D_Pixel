using System.Collections;
using UnityEngine;

public class InvisibleSkill : MonoBehaviour
{
    [SerializeField] private float _invisibleDuration = 25f;
    [SerializeField] private float _cooldownDuration = 60f;

    private SpriteRenderer _spriteRenderer;
    public bool _isInvisible;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }


    // public IEnumerator ToggleInvisibility()
    // {
    //     _isInvisible = !_isInvisible;

    //     _spriteRenderer.sprite =_isInvisible ? invisibleSprite : normalSprite;
    //     yield return new WaitForSeconds(_invisibleDuration);
    // }

    // private IEnumerator InvisibleRoutine()
    // {
    //     _isInvisible = true;
    //     _spriteRenderer.sprite = invisibleSprite;

    //     yield return new WaitForSeconds(_invisibleDuration);

    //     _isInvisible = false;
    //     _spriteRenderer.sprite = normalSprite;
    // }
}
