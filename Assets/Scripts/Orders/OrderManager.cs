using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour

{
    public static OrderManager Instance;

    public Order currentOrder;

    private void Awake()
    {
        Instance = this;
    }

    public void CreateRandomOrder()
    {
        currentOrder = new Order();

        currentOrder.coffeeType =
            (CoffeeType)Random.Range(0, 4);

        currentOrder.reward =
            Random.Range(20, 41);

        currentOrder.timeLimit = 90;

        Debug.Log("Yeni Sipariş: " + currentOrder.coffeeType);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            CreateRandomOrder();
        }
    }
}