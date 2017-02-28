temp = list.files(pattern="*.csv")
for (i in 1:length(temp)) assign(temp[i], read.csv(temp[i], header=FALSE))

#toString in order to transfer black white or rndCue

for (participant in 111:136)
{
  dataframe_of_content1 = paste('PLR',participant,'left',sep='')
  dataframe_of_content2 = paste('PLR',participant,'right',sep='')
  dataframe_of_content3 = paste('PLR',participant,'rndCue',sep='')
  temp = data.frame(M1=double(),
                    M2=double(),
                    D1=double(),
                    D2=double())
  assign(dataframe_of_content1,temp)
  assign(dataframe_of_content2,temp)
  assign(dataframe_of_content3,temp)
  for (trial in 10:60)
  {
  name = paste('PLR',participant,'_','trial',trial,'_data.csv', sep='')
  info_name = paste('PLR',participant,'_','trial',trial,'_info.csv', sep='')
  if (trial==10)
  {
    previous_name = paste('PLR',participant-1,'_','trial',60,'_data.csv', sep='')
  }
  
  else
  {
    previous_name = paste('PLR',participant,'_','trial',trial-1,'_data.csv', sep='')
  }
  number=as.integer(get(info_name)$V1)
  vector1_begin=get(name)$V2[1:floor(number/2+5)]
  vector1_previous=tail(get(previous_name)$V2,750)
  vector1=c(vector1_previous,vector1_begin)
  color = as.integer(get(info_name)$V3)
  vector2=get(name)$V2[floor(number/2)+5:floor(color/2+5)]
  M1=mean(vector1)
  M2=mean(vector2)
  D1=var(vector1)
  D2=var(vector2)
  shown = toString(get(info_name)$V2)
  newrow=c(M1,M2,D1,D2)
  generate = paste('PLR',participant,shown,sep='')
  temp = data.frame(get(generate))
  temp=rbind(temp, newrow)
  assign(generate, temp)
  }
}

list_to_output=list()
list_of_names=c('')
three=c('left','right','rndCue')
for (i in 111:136)
{
  for (item in three)
  {
    name = paste('PLR',i,item,sep="")
    frame = get(paste('PLR',i,item,sep=""))
    list_to_output[[name]] = frame
    list_of_names = c(list_of_names,name)
  }
}

  
lapply(1:length(list_to_output), function(i) write.csv(list_to_output[[i]], 
                                                file = paste0(names(list_to_output[i]), ".csv"),
                                                row.names = FALSE))

for (participant in 111:136)
{
  for (trial in 10:60)
  {
    name = paste('PLR',participant,'_','trial',trial,'_data.csv', sep='')
    info_name = paste('PLR',participant,'_','trial',trial,'_info.csv', sep='')
    
  }
}

features = data.frame(matrix(NA, nrow = 1, ncol = 900))
y = data.frame(matrix(NA, nrow = 1, ncol = 1))
for (participant in 111:136)
{
  for (trial in 10:60)
  {
    name = paste('PLR',participant,'_','trial',trial,'_data.csv', sep='')
    info_name = paste('PLR',participant,'_','trial',trial,'_info.csv', sep='')
    number = as.integer(get(info_name)$V3)
    begin = floor(number/2+5)
    end = floor(number/2+5)+900
    vector = get(name)$V2[begin:end]
    features = rbind(features, c(vector))
    predictor = toString(get(info_name)$V4)
    y = rbind(y, c(predictor))
  }
}

features = features[-1,]
y = y[-1,]

library(caret)

set.seed(123)
all = data.frame(features, y)
smp_size <- floor(0.75 * nrow(all))
train_ind <- sample(seq_len(nrow(all)), size = smp_size)
train = all[train_ind, ]
test = all[-train_ind, ]
length(test)
library(randomForest)
rf = randomForest(y~ ., data=train, importance=TRUE,proximity=TRUE)
prediction = as.vector(predict (rf, test[,1:900] ))
true = as.vector(test$y)
sum=0
for (i in 1:length(prediction))
{
  if (prediction[i]==true[i])
  {
   sum = sum+1 
  }
}

sum/length(prediction)

features2 = data.frame(matrix(NA, nrow = 1, ncol = 1950))
y2 = data.frame(matrix(NA, nrow = 1, ncol = 1))
for (participant in 111:136)
{
  for (trial in 10:60)
  {
    name = paste('PLR',participant,'_','trial',trial,'_data.csv', sep='')
    info_name = paste('PLR',participant,'_','trial',trial,'_info.csv', sep='')
    number2 = as.integer(get(info_name)$V3)
    end = floor(number2/2+5)
    begin = end - 1950
    vector = get(name)$V2[begin:end]
    features2 = rbind(features2, c(vector))
    predictor = toString(get(info_name)$V2)
    y2 = rbind(y2, c(predictor))
  }
}
features2 = features2[-1,]
y2 = y2[-1,]

set.seed(123)
all2 = data.frame(features2, y2)
smp_size2 <- floor(0.75 * nrow(all2))
train_ind2 <- sample(seq_len(nrow(all2)), size = smp_size2)
train2 = all2[train_ind, ]
train2$y2
test2 = all2[-train_ind, ]
length(test2)
rf2 = randomForest(y2~ ., data=train2, importance=TRUE,proximity=TRUE)
prediction2 = as.vector(predict (rf2, test2[,1:1950] ))
true2 = as.vector(test2$y)
sum=0
for (i in 1:length(prediction2))
{
  if (prediction2[i]==true2[i])
  {
    sum = sum+1 
  }
}

sum/length(prediction2)
test2[,1:1950]