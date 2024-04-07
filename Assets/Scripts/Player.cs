using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("Speed")]
    private int speed = 5;

    [SerializeField]
    [FormerlySerializedAs("HpBar")]
    private GameObject hpBar;

    [FormerlySerializedAs("ScoreText")]
    [SerializeField]
    private GameObject scoreText;

    [FormerlySerializedAs("ReplayButton")]
    [SerializeField]
    private Button replayButton;

    private readonly int _hurt = Animator.StringToHash("hurt");
    private readonly int _run = Animator.StringToHash("run");

    private Animator _animator;
    private AudioSource _audioSource;
    private GameObject _currentFloor;
    private ReactiveProperty<int> _hp;
    private int _scope;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _hp = new ReactiveProperty<int>(10);
        _scope = 0;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = transform.GetComponent<AudioSource>();

        SubscribeMove();
        SubscribeScoreUpdate();
        SubscribeUpdateHp();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (OnNormal(other))
        {
            OnNormalFloor(other);
        }
        else if (OnNails(other))
        {
            OnNailds(other);
        }
        if (OnCelling(other))
        {
            TouchCelling(other);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag is "DeathLine")
        {
            ModifyHp(-10);
        }
    }

    private bool OnCelling(Collision2D other)
    {
        return other.gameObject.tag is "Celling";
    }

    private bool OnNails(Collision2D other)
    {
        return other.gameObject.tag is "Nails" && other.contacts[0].normal == Vector2.up;
    }

    private bool OnNormal(Collision2D other)
    {
        return other.gameObject.tag is "Normal" && other.contacts[0].normal == Vector2.up;
    }

    private void TouchCelling(Collision2D other)
    {
        _currentFloor.GetComponent<BoxCollider2D>().enabled = false;
        Hurt();
        other.gameObject.GetComponent<AudioSource>().Play();
    }

    private void OnNailds(Collision2D other)
    {
        SetCurrentFloor(other);
        Hurt();
        other.gameObject.GetComponent<AudioSource>().Play();
    }

    private void OnNormalFloor(Collision2D other)
    {
        SetCurrentFloor(other);
        _animator.SetBool(_hurt, false);
        ModifyHp(1);
        other.gameObject.GetComponent<AudioSource>().Play();
    }

    private void SubscribeUpdateHp()
    {
        _hp.Subscribe(_ => UpdateHpBar()).AddTo(this);
        _hp.Where(r => r <= 0).Subscribe(_ => GameOver()).AddTo(this);
    }

    private void SubscribeScoreUpdate()
    {
        Observable.EveryUpdate()
            .Select(_ => Time.deltaTime)
            .Scan((total, delta) => total + delta >= 2 ? 0 : total + delta)
            .Where(total => total == 0)
            .Subscribe(_ => UpdateScope())
            .AddTo(this);
    }

    private void SubscribeMove()
    {
        Observable.EveryUpdate().Subscribe(_ => Move()).AddTo(this);
    }

    private void Move()
    {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            MoveRight();
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            MoveLeft();
        }
        else
        {
            Stand();
        }
    }

    private void Stand()
    {
        _animator.SetBool(_run, false);
    }

    private void MoveLeft()
    {
        transform.Translate(-speed * Time.deltaTime, 0, 0);
        _spriteRenderer.flipX = false;
        _animator.SetBool(_run, true);
    }

    private void MoveRight()
    {
        transform.Translate(speed * Time.deltaTime, 0, 0);
        _spriteRenderer.flipX = true;
        _animator.SetBool(_run, true);
    }

    private void ModifyHp(int value)
    {
        var hp = _hp.Value + value;
        _hp.Value = hp switch
        {
            < 0 => 0,
            > 10 => 10,
            _ => hp
        };
    }

    private void GameOver()
    {
        Time.timeScale = 0;
        replayButton.gameObject.SetActive(true);
        _audioSource.Play();
    }

    private void Hurt()
    {
        _animator.SetTrigger(_hurt);
        ModifyHp(-3);
    }

    private void UpdateScope()
    {
        _scope++;
        scoreText.GetComponent<TMP_Text>().text = $"Score: {_scope}";
    }

    private void UpdateHpBar()
    {
        for(var index = 0; index < hpBar.transform.childCount; index++)
        {
            hpBar.transform.GetChild(index).gameObject.SetActive(_hp.Value > index);
        }
    }

    private void SetCurrentFloor(Collision2D other)
    {
        if (other.contacts[0].normal == Vector2.up)
        {
            _currentFloor = other.gameObject;
        }
    }

    public void Replay()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("SampleScene");
    }
}
