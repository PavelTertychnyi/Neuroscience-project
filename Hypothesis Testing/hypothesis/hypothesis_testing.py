import csv
import os
from scipy import stats
import numpy as np


def parse_file(file):
    with open(file) as f:
        data = [line.strip() for line in f.readlines()]
    return data


def degreesOfFreedom(X, Y):
    n = len(X)
    m = len(Y)
    d1 = np.var(X)
    d2 = np.var(Y)
    freedom = int((d1 / n + d2 / m) * (d1 / n + d2 / m) / (d1 * d1 / (n * n * (n - 1)) + (d2 * d2 / (m * m * (m - 1)))))
    return freedom


def F_test(X, Y):
    F = np.var(X) / np.var(Y)
    df1 = len(X) - 1
    df2 = len(Y) - 1
    alpha = 0.05
    p_value = stats.f.cdf(F, df1, df2)
    if p_value > alpha:
        # Reject the null hypothesis that Var(X) == Var(Y)
        return False
    return True


def test_hypotesis(X, type1, Y, type2):
    if F_test(X, Y):
        t = stats.ttest_ind(X, Y, equal_var=True)[0]
        freedom = len(X) + len(Y) - 2
    else:
        t = stats.ttest_ind(X, Y, equal_var=False)[0]
        freedom = degreesOfFreedom(X, Y)
    if not (type1 == "right" and type2 == "left") or (type2 == "right" and type1 == "left"):
        alpha = 0.05
        ta = getQuantile(alpha, freedom)
        if t < ta:
            return True
        else:
            return False
    else:
        alpha = 0.025
        ta = getQuantile(alpha, freedom)
        if abs(t) < ta:
            return True
        else:
            return False


def getQuantile(alpha, freedom):
    table = parse_file('t_table.txt')
    quantile = 0
    if alpha == 0.05:
        quantile = table[freedom - 1].split('\t')[5]
    elif alpha == 0.025:
        quantile = table[freedom - 1].split("\t")[6]
    return quantile


def participant_stat(participant_id, cue, folder=os.getcwd()):
    data = []
    f = os.path.join(folder, 'PLR'+participant_id+cue+'.csv')
    with open(f) as csv_file:
        plr_reader = csv.reader(csv_file, delimiter=' ')
        for row in plr_reader:
            row = ''.join(row)
            row = row.split(',')
            try:
                data.append(float(row[1]))
            except ValueError:
                pass
        return data[1:]


def test_participants(folder=os.getcwd()):
    participants = [i for i in xrange(111, 137)]
    for participant in participants:
        right = participant_stat(str(participant), "right", folder)
        left = participant_stat(str(participant), "left", folder)
        random = participant_stat(str(participant), "rndCue", folder)
        print "Participant number: ", participant
        print "Hypothesis right - random mean equality: "
        print np.mean(right), np.   mean(random)
        print test_hypotesis(right, "right", random, "rndCue")
        print
        print "Hypothesis left - random mean equality: "
        print np.mean(left), np.mean(random)
        print test_hypotesis(left, "left", random, "rndCue")
        print
        print "Hypothesis right - left mean equality: "
        print np.mean(right), np.mean(left)
        print test_hypotesis(right, "right", left, "left")
        print


test_participants("1")







