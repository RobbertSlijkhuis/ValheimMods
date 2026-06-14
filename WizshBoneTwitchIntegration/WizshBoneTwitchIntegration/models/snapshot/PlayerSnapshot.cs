namespace WizshBoneTwitchIntegration.Models
{
    internal class PlayerSnapshot
    {
        public float jumpForce;
        public float jumpForceForward;

        public float acceleration;
        public float crouchSpeed;
        public float runSpeed;
        public float speed;
        public float walkSpeed;

        public float swimAcceleration;
        public float swimDepth;
        public float swimSpeed;

        public float maxCarryWeight;

        public PlayerSnapshot(Player player)
        {
            Create(player);
        }

        public void Create(Player player)
        {
            jumpForce = player.m_jumpForce;
            jumpForceForward = player.m_jumpForceForward;

            acceleration = player.m_acceleration;
            crouchSpeed = player.m_crouchSpeed;
            runSpeed = player.m_runSpeed;
            speed = player.m_speed;
            walkSpeed = player.m_walkSpeed;

            swimAcceleration = player.m_swimAcceleration;
            swimDepth = player.m_swimDepth;
            swimSpeed = player.m_swimSpeed;

            maxCarryWeight = player.m_maxCarryWeight;
        }

        public void Apply(Player player)
        {
            player.m_jumpForce = jumpForce;
            player.m_jumpForceForward = jumpForceForward;

            player.m_acceleration = acceleration;
            player.m_crouchSpeed = crouchSpeed;
            player.m_runSpeed = runSpeed;
            player.m_speed = speed;
            player.m_walkSpeed = walkSpeed;

            player.m_swimAcceleration = swimAcceleration;
            player.m_swimDepth = swimDepth;
            player.m_swimSpeed = swimSpeed;

            player.m_maxCarryWeight = maxCarryWeight;
        }
    }
}
