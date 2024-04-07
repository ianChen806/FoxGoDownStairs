using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private static readonly int _hurt = Animator.StringToHash("hurt");

    [SerializeField]
    [FormerlySerializedAs("Speed")]
    private int speed = 5;

    [SerializeField]
    [FormerlySerializedAs("HpBar")]
    private GameObject hpBar;

    [FormerlySerializedAs("ScoreText")]
    [SerializeField]
    private GameObject scoreText;

    [FormerlySerializedAs("replayButton")]
    [FormerlySerializedAs("ReplayButton")]
    [SerializeField]
    private Button _replayButton;

    private readonly int _run = Animator.StringToHash("run");

    private Animator _animator;
    private AudioSource _audioSource;
    private GameObject _currentFloor;
    private int _hp = 5;
    private int _scope;
    private float _scoreTime;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        Debug.Log("Start");
        ModifyHp(10);
        _scope = 0;
        _scoreTime = 0;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = transform.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.Translate(speed * Time.deltaTime, 0, 0);
            _spriteRenderer.flipX = true;
            _animator.SetBool(_run, true);
        }
        else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            transform.Translate(-speed * Time.deltaTime, 0, 0);
            _spriteRenderer.flipX = false;
            _animator.SetBool(_run, true);
        }
        else
        {
            _animator.SetBool(_run, false);
        }
        UpdateScope();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag is "Normal" && other.contacts[0].normal == Vector2.up)
        {
            SetCurrentFloor(other);
            _animator.SetBool(_hurt, false);
            ModifyHp(_hp + 1);
            other.gameObject.GetComponent<AudioSource>().Play();
        }
        else if (other.gameObject.tag is "Nails" && other.contacts[0].normal == Vector2.up)
        {
            SetCurrentFloor(other);
            Hurt();
            other.gameObject.GetComponent<AudioSource>().Play();
        }
        if (other.gameObject.tag is "Celling")
        {
            _currentFloor.GetComponent<BoxCollider2D>().enabled = false;
            Hurt();
            other.gameObject.GetComponent<AudioSource>().Play();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag is "DeathLine")
        {
            GameOver();
        }
    }

    private void ModifyHp(int value)
    {
        _hp = value switch
        {
            < 0 => 0,
            > 10 => 10,
            _ => value
        };
        if (_hp == 0)
        {
            GameOver();
        }
        Debug.Log(_hp);
        UpdateHpBar();
    }

    private void GameOver()
    {
        Time.timeScale = 0;
        _replayButton.gameObject.SetActive(true);
        Debug.Log("Game Over");
        _audioSource.Play();
    }

    private void Hurt()
    {
        _animator.SetTrigger(_hurt);
        ModifyHp(_hp - 3);
    }

    private void UpdateScope()
    {
        _scoreTime += Time.deltaTime;
        if (_scoreTime > 2)
        {
            _scope++;
            _scoreTime = 0;
            scoreText.GetComponent<TMP_Text>().text = $"Score: {_scope}";
        }
    }

    private void UpdateHpBar()
    {
        for(var index = 0; index < hpBar.transform.childCount; index++)
        {
            hpBar.transform.GetChild(index).gameObject.SetActive(_hp > index);
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
