using System;
using System.Linq;
using UnityEngine;
using ZooWorld.Core.Animals;
using ZooWorld.Game.Configs;
using ZooWorld.Game.Movement;

namespace ZooWorld.Game.Animals
{
    /// <summary>
    /// Unity glue for one animal: owns the Rigidbody, forwards contacts and
    /// raises events for views. It holds no game rules; those live in Core.
    /// The look lives on a child "Visual" object that artists can replace freely.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Animal : MonoBehaviour, IMovementBody
    {
        /// <summary>Name of the child that holds the look. Nothing under it affects logic or physics.</summary>
        public const string VisualName = "Visual";

        [Tooltip("How fast the animal turns to face its heading, in degrees per second.")]
        [SerializeField, Min(0f)] private float _turnSpeed = 720f;

        private Rigidbody _body;
        private Collider[] _colliders;
        private IDeathAnimation[] _deathAnimations;
        private IAnimalContactListener _contacts;

        public event Action Spawned;
        public event Action Ate;
        public event Action Died;

        /// <summary>Raised when the screen edge or a bump changes the heading, for views to react.</summary>
        public event Action<Redirect> Redirected;

        public AnimalId Id { get; private set; }
        public AnimalConfig Config { get; private set; }
        public IMovement Movement { get; private set; }
        public bool IsAlive { get; private set; }

        public Vector3 Position => _body.position;

        public Vector3 Velocity
        {
            get => Planar.Flatten(_body.linearVelocity);
            set => _body.linearVelocity = Planar.Flatten(value);
        }

        private void Awake() => CacheComponents();

        public void Activate(AnimalId id, AnimalConfig config, IMovement movement,
            IAnimalContactListener contacts, Vector3 position)
        {
            // Awake doesn't run for objects created in Edit mode (e.g. contract tests).
            if (_body == null) CacheComponents();

            Id = id;
            Config = config;
            Movement = movement;
            _contacts = contacts;

            var rotation = Quaternion.LookRotation(movement.Heading);
            transform.SetPositionAndRotation(position, rotation);
            _body.position = position;
            _body.rotation = rotation;
            _body.isKinematic = false;
            _body.linearVelocity = Vector3.zero;
            SetCollidersEnabled(true);

            IsAlive = true;
            Spawned?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            Movement.Tick(this, deltaTime);

            var facing = Quaternion.LookRotation(Movement.Heading);
            _body.MoveRotation(Quaternion.RotateTowards(_body.rotation, facing, _turnSpeed * deltaTime));
        }

        /// <summary>Turns the animal back towards the play area.</summary>
        public void Steer(Vector3 heading)
        {
            var before = Movement.Heading;
            Movement.Steer(heading);
            Redirected?.Invoke(new Redirect(RedirectCause.ScreenEdge, Vector3.SignedAngle(before, Movement.Heading, Vector3.up)));
        }

        /// <summary>Physics has just bounced the animal; its movement follows the bounce.</summary>
        public void Bump()
        {
            var before = Movement.Heading;
            Movement.OnBumped(this);
            Redirected?.Invoke(new Redirect(RedirectCause.Bump, Vector3.SignedAngle(before, Movement.Heading, Vector3.up)));
        }

        public void NotifyAte() => Ate?.Invoke();

        /// <summary>
        /// Takes the animal out of play at once; <paramref name="onRecyclable"/> fires
        /// when its death animations (if any) are over and the object can be pooled.
        /// </summary>
        public void Kill(Action onRecyclable)
        {
            IsAlive = false;
            _contacts = null;
            _body.linearVelocity = Vector3.zero;
            _body.isKinematic = true;
            SetCollidersEnabled(false);
            Died?.Invoke();

            // Only views that are actually running get to delay recycling.
            var playing = _deathAnimations.Where(animation => !(animation is Behaviour view) || view.isActiveAndEnabled).ToArray();
            var pending = playing.Length;
            if (pending == 0)
            {
                onRecyclable();
                return;
            }

            foreach (var animation in playing)
            {
                animation.Play(() =>
                {
                    if (--pending == 0) onRecyclable();
                });
            }
        }

        /// <summary>Shows or hides the look. Headless runs hide it, so no view runs.</summary>
        public void SetVisualEnabled(bool isEnabled)
        {
            var visual = transform.Find(VisualName);
            if (visual != null) visual.gameObject.SetActive(isEnabled);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsAlive || _contacts == null) return;

            var other = collision.rigidbody != null ? collision.rigidbody.GetComponent<Animal>() : null;
            _contacts.OnContact(this, other);
        }

        private void CacheComponents()
        {
            _body = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>(true);
            _deathAnimations = GetComponentsInChildren<IDeathAnimation>(true);
        }

        private void SetCollidersEnabled(bool isEnabled)
        {
            foreach (var collider in _colliders) collider.enabled = isEnabled;
        }

        private void Reset()
        {
            var body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.linearDamping = 3f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
    }
}
