namespace BearBonesMessaging.RabbitMq.RabbitMqManagement
{
    /// <summary>
    /// User detail list as returned by Get("/api/users/")
    /// See http://www.rabbitmq.com/management.html
    /// </summary>
    public interface IRMUser
    {
#pragma warning disable 1591
        string name { get; set; }
        string password_hash { get; set; }
        string hashing_algorithm { get; set; }
        string tags { get; set; }
#pragma warning restore 1591
    }
}