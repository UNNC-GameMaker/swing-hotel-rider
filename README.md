## Swing Hotel (Somehow Rider Compatiable Version)

分支命名规范：
文件和branch名不准用中文
```
feature/<short-description>
fix/<short-description>
hotfix/<short-description>
chore/<short-description>
```

提交信息：
类别：详细信息
```
feat: add player movement
fix: resolve null ref in enemy AI
chore: update .gitignore
style: rename scripts to follow naming rules
```


本项目 Unity 版本：2022.3.62f3（请务必保持一致）

---

## 代码命名规范

### 文件命名
- **脚本文件**: PascalCase，与类名完全一致
  - ✅ `PlayerMovement.cs`
  - ✅ `GameManager.cs`
  - ❌ `playerMovement.cs`
  - ❌ `Player_Movement.cs`

- **场景文件**: PascalCase
  - ✅ `MainMenu.unity`
  - ✅ `Level01.unity`
  - ❌ `main_menu.unity`

- **预制体（Prefab）**: PascalCase
  - ✅ `Player.prefab`
  - ✅ `EnemySpawner.prefab`
  - ❌ `player.prefab`

- **资源文件**: kebab-case（小写，连字符分隔）
  - ✅ `player-idle.png`
  - ✅ `background-music.mp3`
  - ❌ `PlayerIdle.png`

### C# 代码命名

#### 类（Class）
- **规则**: PascalCase
- **示例**:
```csharp
public class PlayerController { }
public class EnemyAI { }
public class GameManager { }
```

#### 接口（Interface）
- **规则**: PascalCase，以 `I` 开头
- **示例**:
```csharp
public interface IInteract { }
public interface IDamageable { }
public interface IPoolable { }
```

#### 抽象类（Abstract Class）
- **规则**: PascalCase，可选择性添加 `Base` 后缀
- **示例**:
```csharp
public abstract class Grabbable { }
public abstract class WeaponBase { }
public abstract class Character { }
```

#### 方法（Method）
- **规则**: PascalCase
- **示例**:
```csharp
public void MovePlayer() { }
private void CalculateDamage() { }
protected void OnTriggerEnter() { }
```

#### 属性（Property）
- **规则**: PascalCase
- **示例**:
```csharp
public int Health { get; set; }
public bool IsGrounded { get; private set; }
public float MaxSpeed => 10f;
```

#### 字段（Field）

**公共字段（不推荐）**:
- **规则**: PascalCase
- **示例**:
```csharp
public float MoveSpeed = 5f;
public GameObject Target;
```

**私有字段（推荐使用 SerializeField）**:
- **规则**: camelCase，以 `_` 开头
- **示例**:
```csharp
[SerializeField] private float _moveSpeed = 5f;
[SerializeField] private GameObject _targetPrefab;
private Rigidbody2D _rb;
private bool _isGrounded;
```

**常量**:
- **规则**: PascalCase
- **示例**:
```csharp
private const int MaxHealth = 100;
private const float Gravity = -9.81f;
public const string PlayerTag = "Player";
```

**静态只读字段**:
- **规则**: PascalCase
- **示例**:
```csharp
public static readonly Vector3 DefaultPosition = Vector3.zero;
private static readonly int AnimationHash = Animator.StringToHash("Run");
```

#### 局部变量和参数
- **规则**: camelCase
- **示例**:
```csharp
void CalculateDistance(Vector3 startPosition, Vector3 endPosition)
{
    float distance = Vector3.Distance(startPosition, endPosition);
    int roundedValue = Mathf.RoundToInt(distance);
}
```

#### 枚举（Enum）
- **规则**: PascalCase（枚举类型和值都是）
- **示例**:
```csharp
public enum MovementState
{
    Idle,
    Walking,
    Running,
    Jumping
}

public enum WeaponType
{
    Melee,
    Ranged,
    Magic
}
```

#### 事件（Event）
- **规则**: PascalCase，使用动词或动词短语
- **示例**:
```csharp
public event Action OnPlayerDeath;
public event Action<int> OnScoreChanged;
public event Action<float, float> OnHealthUpdated;
```

#### 委托（Delegate）
- **规则**: PascalCase，通常以 `Handler` 或 `Callback` 结尾
- **示例**:
```csharp
public delegate void DamageHandler(int damage);
public delegate bool ValidationCallback(string input);
```

#### 命名空间（Namespace）
- **规则**: PascalCase，层次结构用 `.` 分隔
- **示例**:
```csharp
namespace SwingHotel.Player { }
namespace SwingHotel.Managers { }
namespace SwingHotel.UI.Menus { }
namespace GameObjects { }
```

### Unity 特定命名

#### MonoBehaviour 回调方法
- **保持 Unity 原有命名**:
```csharp
private void Awake() { }
private void Start() { }
private void Update() { }
private void FixedUpdate() { }
private void OnEnable() { }
private void OnDisable() { }
private void OnTriggerEnter2D(Collider2D other) { }
private void OnCollisionEnter2D(Collision2D collision) { }
```

#### Animator 参数
- **规则**: camelCase
- **示例**:
```csharp
animator.SetBool("isRunning", true);
animator.SetTrigger("jump");
animator.SetFloat("speed", 5f);
```

#### Unity Tags 和 Layers
- **规则**: PascalCase
- **示例**:
  - Tags: `"Player"`, `"Enemy"`, `"Grabbable"`, `"Interact"`
  - Layers: `"Ground"`, `"PlayerLayer"`, `"EnemyLayer"`

#### Scene GameObject 命名
- **规则**: PascalCase，描述性命名
- **示例**:
  - ✅ `Player`
  - ✅ `MainCamera`
  - ✅ `EnemySpawnPoint_01`
  - ✅ `UI_Canvas`

### 代码组织规范

#### Region 使用
```csharp
public class Example : MonoBehaviour
{
    #region Inspector Fields
    [SerializeField] private float _speed = 5f;
    [SerializeField] private GameObject _prefab;
    #endregion
    
    #region Private Fields
    private Rigidbody2D _rb;
    private bool _isActive;
    #endregion
    
    #region Public Properties
    public bool IsActive => _isActive;
    public float Speed => _speed;
    #endregion
    
    #region Unity Callbacks
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    #endregion
    
    #region Public Methods
    public void Activate() { }
    public void Deactivate() { }
    #endregion
    
    #region Private Methods
    private void CalculateMovement() { }
    private void HandleInput() { }
    #endregion
}
```
