﻿using System;

using Game.States;
using SevenEngine.Imaging;
using SevenEngine.Mathematics;
using SevenEngine.DataStructures;
using SevenEngine.StaticModels;

namespace Game.Units
{
  public class KillemMelee : Melee
  {
    Unit _target;
    float _time = 0;
    float _delay = 0;
    int move;
    bool attack;

    public KillemMelee(string id, StaticModel staticModel) : base(id, staticModel) { _time = 0; if (AiBattle._map == 0) _delay = 4000; }

    public override void AI(float elapsedTime, OctreeLinked<Unit, string> octree)
    {
      if (_time <= _delay)
        _time += elapsedTime;
      if (IsDead == false)
      {
        if (_target == null || _target.IsDead || move > 20)
        {
          attack = false;
          float shortest = float.MaxValue;
          move = 0;
          octree.Traverse
          (
            (Unit current) =>
            {
              if ((current is ZackMelee || current is ZackRanged) && !current.IsDead)
              {
                if (_target == null || _target.IsDead || _target is KillemKamakazi)
                {
                  _target = current;
                  shortest = (current.Position - Position).LengthSquared();
                }
                else
                {
                  float length = (current.Position - Position).LengthSquared();
                  if (length < shortest)
                  {
                    _target = current;
                    shortest = length;
                  }
                }
              }
              else if (current is ZackKamakazi && !current.IsDead)
              {
                if (_target == null || _target.IsDead)
                {
                  _target = current;
                  shortest = (current.Position - Position).LengthSquared();
                }
                else
                {
                  float length = (current.Position - Position).LengthSquared();
                  if (length < shortest)
                  {
                    _target = current;
                    shortest = length;
                  }
                }
              }
            }
          );
        }
        // Attacking
        else if (Calc.Abs((Position - _target.Position).LengthSquared()) < _attackRangedSquared)
        {
          if (!attack)
            AiBattle.lines.TryAdd(new Link3<Vector, Vector, Color>(
              new Vector(Position.X, Position.Y, Position.Z),
              new Vector(_target.Position.X, _target.Position.Y, _target.Position.Z),
              Color.Blue));
          if (Attack(_target))
            _target = null;
          move = 0;
          attack = !attack;
        }
        // Moving
        else if (_time > _delay)
        {
          Position = Vector.MoveTowardsPosition(Position, _target.Position, MoveSpeed);
          move++;
          attack = false;
        }
        this.StaticModel.Orientation.W += .1f;
      }
    }
  }
}