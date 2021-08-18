var express = require('express');
var router = express.Router();
var mongoose = require('mongoose');
var mongoConn = require('../config/mongoPool')();
var mongoSchema = require('../config/mongoSchema')();

mongoConn.mongoConnect(mongoose);
var db = mongoConn.mongoConnection(mongoose);
var WritingData = mongoSchema.writingData(mongoose);

router.get('/', function(req, res) { 
    res.render('index', { title: "수집서버 시작" });
});


router.get('/mongoInsert', function(req, res) { 
  
    
    var StudentModel = mongoSchema.getStudent(mongoose);

    // 3. Student 객체를 new 로 생성해서 값을 입력
    var newStudent = new StudentModel({name:'Hong Gil Dong', address:'서울시 강남구 논현동', age:'22'});

    // 4. 데이터 저장
    newStudent.save(function(error, data){
        if(error){
            console.log(error);
        }else{
            console.log('Saved!')
        }

        var returnObject = new Object();
        returnObject.code = 0;
        returnObject.msg = "successInsert";

        res.send(returnObject);
    });

});

router.get('/mongoSelectAll', function(req, res) { 

    var StudentModel = mongoSchema.getStudent(mongoose);

    // 5. Student 레퍼런스 전체 데이터 가져오기
    StudentModel.find(function(error, students){
        console.log('--- Read all ---');
        if(error){
            console.log(error);
        }else{
            console.log(students);
        }
        var returnObject = new Object();
        returnObject.code = 0;
        returnObject.msg = "successSelectAll";
        
        res.send(returnObject);
    })


    // // 6. 특정 아이디값 가져오기
    // Student.findOne({_id:'585b777f7e2315063457e4ac'}, function(error,student){
    //     console.log('--- Read one ---');
    //     if(error){
    //         console.log(error);
    //     }else{
    //         console.log(student);
    //     }
    // });

    // // 7. 특정 아이디값 가져오기
    // Student.findOne({_id:'585b777f7e2315063457e4ac'}, function(error,student){
    //     console.log('--- Read one ---');
    //     if(error){
    //         console.log(error);
    //     }else{
    //         console.log(student);
    //     }
    // });

    // // 8. 특정아이디 수정하기
    // Student.findById({_id:'585b777f7e2315063457e4ac'}, function(error,student){
    //     console.log('--- Update(PUT) ---');
    //     if(error){
    //         console.log(error);
    //     }else{
    //         student.name = '--modified--';
    //         student.save(function(error,modified_student){
    //             if(error){
    //                 console.log(error);
    //             }else{
    //                 console.log(modified_student);
    //             }
    //         });
    //     }
    // });

    // // 9. 삭제
    // Student.remove({_id:'585b7c4371110029b0f584a2'}, function(error,output){
    //     console.log('--- Delete ---');
    //     if(error){
    //         console.log(error);
    //     }

    //     /* ( SINCE DELETE OPERATION IS IDEMPOTENT, NO NEED TO SPECIFY )
    //         어떤 과정을 반복적으로 수행 하여도 결과가 동일하다. 삭제한 데이터를 다시 삭제하더라도, 존재하지 않는 데이터를 제거요청 하더라도 오류가 아니기 때문에
    //         이부분에 대한 처리는 필요없다. 그냥 삭제 된것으로 처리
    //         */
    //     console.log('--- deleted ---');
    // });

   // pool.close();
})


router.post('/test', function(req, res) {

    console.log("테스트");
    console.log(req.body);

});

router.post('/saveData', function(req, res) {

    console.log("데이터 저장하기");

    var userName = req.body.name;
    var userData = req.body.data;
    var userPhoneme = req.body.phoneme;

    // 3. WritingData 객체를 new 로 생성해서 값을 입력
    var newWriting = new WritingData({name: userName, phoneme: userPhoneme, data: userData});

    // 4. 데이터 저장
    newWriting.save(function(error, data){
        if(error){
            console.log(error);
        }else{
            console.log('Saved!')
        }

        var returnObject = new Object();
        returnObject.code = 0;
        returnObject.msg = "successInsert";

        res.send(returnObject);
    });

});

router.post('/loadData', function(req, res) {

    console.log("데이터 불러오기");
    
    var userPhoneme = req.body.phoneme;

    WritingData.find({"phoneme":userPhoneme},function(error, writing){
        console.log('--- Read all ---');
        if(error){
            console.log(error);
        }else{
            console.log("Data Lenght: " , writing.length);
            var returnObject = new Object();
            var returnArray = new Array();
            
            for(var i = 0; i < writing.length; i++) {
                var oneObject = new Object();
                oneObject.data = writing[i].data;
                oneObject.name = writing[i].name;
                oneObject.id   = writing[i].id;
                returnArray.push(oneObject);
            }
            
            returnObject.length = writing.length;
            returnObject.dataArray = returnArray;
            res.send(returnObject);
        }
    })

});

router.post('/deleteData', function(req, res) {

    console.log("데이터 삭제하기");
    
    var userId = req.body.id;

    // 9. 삭제
    WritingData.remove({_id:userId}, function(error,output){
        console.log('--- Delete ---');
        if(error){
            console.log(error);
        }
    });


});


module.exports = router;