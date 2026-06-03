public static class PdfScenarioTextProvider
{
    public static string GetText(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return string.Empty;
        }

        switch (eventId)
        {
            case "EVAC_01":
                return "イベント1「持ち出すものを選ぶ」\n\n避難する前に、手元にあるものを少しだけ持っていける。\nただし、持ちすぎると移動が遅くなり、体力を消費しやすくなる。";
            case "EVAC_02":
                return "イベント2「倒れた自転車と道を塞ぐ荷物」\n\n細い道で自転車や棚が倒れて道を塞いでいる。\n近くに高齢者と小学生がいて、先に進めず困っている。";
            case "EVAC_03":
                return "イベント3「半壊したコンビニ」\n\nシャッターが半分開いたコンビニがある。\n中には水や食料、乾電池、衛生用品が残っている。\nしかし店内は少し危険そうだ。";
            case "EVAC_04":
                return "イベント4「泣いている子ども」\n\n道路脇で子どもが泣いている。\n親とはぐれたらしく、避難所の場所も分からない様子。";
            case "EVAC_05":
                return "イベント5「水を分けるか、残すか」\n\n避難所まであと少し。\n途中で、喉が渇いて動けない人に出会う。";
            case "EVAC_06":
                return "イベント6「避難所前の最後の坂」\n\n避難所の小学校が見えてくる。\n最後の坂道で、荷物を持った人たちが苦しそうに歩いている。";
            case "SH_01":
                return "イベント01「避難所の受付」\n\n体育館には、すでに多くの人が集まっていた。\n受付には長い列ができ、人手が足りていないようだ。";
            case "SH_02":
                return string.Empty;
            case "SH_03":
                return "イベント03「最初の夜の仕事」\n\n夜になり、避難所では人手が足りなくなってきた。";
            case "SH_04":
                return "イベント04「支援金申請メール」\n\nスマホに一通のメールが届いた。\n公式サイトなのか判断できないリンクがある。";
            case "SH_05":
                return "イベント05「水の配布」\n\n水の配布が始まった。\n全員に十分な量はない。";
            case "SH_06":
                return "イベント06「トイレの問題」\n\n避難所生活が続き、トイレの状態が悪くなってきた。";
            case "SH_07":
                return "イベント07「無料点検の営業」\n\n避難所の入口付近に、作業着姿の男性が現れた。\n書類への記入を求められる。";
            case "SH_08":
                return "イベント08「SNS の物資情報」\n\nSNSで『隣町の避難所は物資が余っている』という投稿が広がっていた。";
            case "SH_09":
                return "イベント09「避難所内の対立」\n\n食料配布の量をめぐって避難者同士が揉めている。";
            case "SH_10":
                return "イベント10「体調不良者」\n\n近くの人が苦しそうにしている。\n職員もすぐには来られそうにない。";
            case "SH_11":
                return "イベント11「救助到着前夜」\n\n救助が近いという話が入った。\n職員が夜間の見回りを手伝える人を探している。";
            default:
                return string.Empty;
        }
    }
}
