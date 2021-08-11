var config = require('../config');

module.exports = function () {
  return {
    test : function(){
      console.log("asdasdas");
    },
    mongoConnect : function(mongoose){
      mongoose.connect('mongodb://'+config.mysqlConfig.host+':'+config.mysqlConfig.port+'/'+ config.mysqlConfig.database);
    },
    mongoConnection : function(mongoose){
      var db = mongoose.connection;
      // 4. 연결 실패
      db.on('error', function(){
          console.log('Connection Failed!');
      });
      // 5. 연결 성공
      db.once('open', function() {
          console.log('Connected!');
      });
      return db;
    }
  }
}