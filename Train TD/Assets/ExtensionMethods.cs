﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class ExtensionMethods {

    public static void ResetTransformation (this Transform trans) {
        trans.position = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = new Vector3(1, 1, 1);
    }

    public static Vector3 vector3 (this Vector2 v2, float z) {
        return new Vector3(v2.x, v2.y, z);
    }

    public static Vector2 vector2 (this Vector3 v3) {
        return new Vector2(v3.x, v3.y);
    }


    public static void DeleteAllChildren(this Transform transform) {
        int childs = transform.childCount;
        for (int i = childs - 1; i >= 0; i--) {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }
}