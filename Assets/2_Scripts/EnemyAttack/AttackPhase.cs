public enum AttackPhase
{
    Approach,
    StandBy,
    Preheat,        // 공격 모션 시작 -> 공격 판정까지 걸리는 시간
    Recovery,       // 공격 모션 종료 후 다시 움직이기 까지 걸리는 시간
}