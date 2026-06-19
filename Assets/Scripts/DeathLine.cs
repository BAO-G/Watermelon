using System.Collections.Generic;
using UnityEngine;

public class DeathLine : MonoBehaviour
{
    private readonly Dictionary<Fruit, float> fruitStayTimes = new Dictionary<Fruit, float>();

    private void OnTriggerStay2D(Collider2D other)
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (!other.TryGetComponent(out Fruit fruit))
        {
            return;
        }

        if (fruit.fruitState == FruitState.Standby)
        {
            return;
        }

        if (!fruitStayTimes.ContainsKey(fruit))
        {
            fruitStayTimes[fruit] = 0f;
        }

        fruitStayTimes[fruit] += Time.deltaTime;

        if (fruitStayTimes[fruit] >= GameManager.Instance.DeathLineStayDuration)
        {
            GameManager.Instance.GameOver();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out Fruit fruit))
        {
            fruitStayTimes.Remove(fruit);
        }
    }

    private void OnDestroy()
    {
        fruitStayTimes.Clear();
    }
}
