namespace WizshBoneTwitchIntegration.Models
{
    internal class PlayerSpeedSnapshot
    {
        public float crouchSpeed;
        public float jumpForce;
        public float jumpForceForward;
        public float runSpeed;
        public float speed;
        public float swimSpeed;
        public float walkSpeed;

        public PlayerSpeedSnapshot(Player player)
        {
            Create(player);
        }

        public void Create(Player player)
        {
            crouchSpeed = player.m_crouchSpeed;
            jumpForce = player.m_jumpForce;
            jumpForceForward = player.m_jumpForceForward;
            runSpeed = player.m_runSpeed;
            speed = player.m_speed;
            swimSpeed = player.m_swimSpeed;
            walkSpeed = player.m_walkSpeed;
        }

        public void Apply(Player player)
        {
            player.m_crouchSpeed = crouchSpeed;
            player.m_jumpForce = jumpForce;
            player.m_jumpForceForward = jumpForceForward;
            player.m_runSpeed = runSpeed;
            player.m_speed = speed;
            player.m_swimSpeed= swimSpeed;
            player.m_walkSpeed= walkSpeed;
        }
    }
}
