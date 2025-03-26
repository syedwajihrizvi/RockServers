import random
from argparse import ArgumentParser

usernames = [
    "ab_dwag",
    "gamer_king",
    "shadow_hawk",
    "night_wolf",
    "xX_sniper_Xx",
    "stealth_ninja",
    "blaze_master",
    "thunder_warrior",
    "venom_strike",
    "rogue_snake",
    "alpha_striker",
    "frost_bite",
    "bullet_rain",
    "cyber_hawk",
    "steel_blade",
    "storm_rider",
    "red_scorpion",
    "nova_commander",
]

def get_three_usernames(exclude_username: str):
    filtered_usernames = set(usernames) - set([exclude_username])
    res = ""
    for _ in range(3):
        choice = random.choice(list(filtered_usernames))
        filtered_usernames.remove(choice)
        res += f"{choice},"
    print(res)

def parse_args():
    parser = ArgumentParser()
    parser.add_argument('-u', '--username', type=str, nargs=1)
    args = parser.parse_args()
    return args.username[0]

username = parse_args()
get_three_usernames(username)