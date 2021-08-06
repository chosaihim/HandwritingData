var config = require('../config');

module.exports = function () {
  return {
    
    getStudent : function(mongoose){
      // 1. Schema 생성. (혹시 스키마에 대한 개념이 없다면, 입력될 데이터의 타입이 정의된 DB 설계도 라고 생각하면 됩니다.)
      var student = mongoose.Schema({
          name : 'string',
          address : 'string',
          age : 'number'
      });

      var StudentModel = mongoose.model('Schema', student);
      return StudentModel;
    },
    
    writingData : function(mongoose){
      var handWriting = mongoose.Schema({
          name : 'string',
          phoneme : 'string',
          data : 'string'
      });

      var HandWriting = mongoose.model('Schema', handWriting);
      return HandWriting;
    }
  
  }
}