using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Microsoft.Xna.Framework;

namespace RTPPlugin
{
    [ApiVersion(2, 1)]
    public class RTPPlugin : TerrariaPlugin
    {
        public static Config Config = new();
        public static readonly List<PlayerCooldown> Cooldowns = new();

        public override string Author => "Даниил";
        public override string Description => "Random Teleport with cooldown";
        public override string Name => "RTPPlugin";
        public override Version Version => new(1, 0, 0);

        public RTPPlugin(Main game) : base(game) { }

        public override void Initialize()
        {
            string path = Path.Combine(TShock.SavePath, "RTP.json");
            Config = Config.Read(path);

            Commands.ChatCommands.Add(new Command("rtp.use", RTPCommand, "rtp"));
            Commands.ChatCommands.Add(new Command("rtp.admin", ResetCooldown, "rtpreset"));
        }

        private void RTPCommand(CommandArgs args)
        {
            var player = args.Player;
            if (player == null || !player.Active) return;

            var cooldown = Cooldowns.FirstOrDefault(c => c.Index == player.Index);
            if (cooldown == null)
            {
                cooldown = new PlayerCooldown(player.Index);
                Cooldowns.Add(cooldown);
            }

            var now = DateTime.UtcNow;
            if ((now - cooldown.LastUsed).TotalMinutes < Config.CooldownMinutes)
            {
                double remaining = Config.CooldownMinutes - (now - cooldown.LastUsed).TotalMinutes;
                player.SendErrorMessage($"Вы сможете использовать RTP через {Math.Ceiling(remaining)} мин.");
                return;
            }

            // Случайные координаты в пределах карты
            int x = Main.rand.Next(100, Main.maxTilesX - 100);
            int y = Main.rand.Next(100, Main.maxTilesY - 100);

            player.Teleport(x * 16, y * 16);
            player.SendSuccessMessage("Вы были телепортированы случайным образом!");

            cooldown.LastUsed = now;
        }

        private void ResetCooldown(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Использование: /rtpreset <имя игрока>");
                return;
            }

            var target = TShock.Players.FirstOrDefault(p => p != null && p.Name.Equals(args.Parameters[0], StringComparison.OrdinalIgnoreCase));
            if (target == null)
            {
                args.Player.SendErrorMessage("Игрок не найден.");
                return;
            }

            var cooldown = Cooldowns.FirstOrDefault(c => c.Index == target.Index);
            if (cooldown != null)
            {
                cooldown.LastUsed = DateTime.MinValue;
                args.Player.SendSuccessMessage($"Таймер RTP для {target.Name} сброшен.");
                target.SendInfoMessage("Админ сбросил ваш RTP таймер.");
            }
            else
            {
                args.Player.SendErrorMessage("У игрока нет активного таймера.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                base.Dispose(disposing);
            }
        }
    }
}