using Spear.Movement;
using UnityEngine;

namespace Spear.Movement
{

    public class AggressiveMovement : IMovementStrategy
    {
        private readonly Transform _transform;
        private readonly float _speed;
        private readonly int _targetLayer;

        public AggressiveMovement(Transform transform, float speed, string layerName = "Demon")
        {
            _transform = transform;
            _speed = speed;
            _targetLayer = LayerMask.NameToLayer(layerName);
        }

        public void Tick()
        {
            GameObject[] all = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            GameObject nearest = null;
            float minDist = float.MaxValue;

            foreach (var go in all)
            {
                if (go.layer != _targetLayer) continue;
                float d = Vector3.Distance(_transform.position, go.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = go;
                }
            }

            if (nearest != null)
            {
                _transform.position = Vector3.MoveTowards(
                    _transform.position,
                    new Vector3(nearest.transform.position.x, _transform.position.y, _transform.position.z),
                    _speed * Time.deltaTime
                );
            }
        }
    }
}