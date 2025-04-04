const express = require('express');
const bodyParser = require('body-parser');
const jwt = require('jsonwebtoken');
const fs = require('fs');

const app = express();
const port = 3000;
const secretKey = 'testkey'; // 실제 서비스에서는 환경변수를 이용

app.use(bodyParser.json());


// 로그인 엔드포인트 (변경된 반환 정보)
app.post('/login', (req, res) => {
  const { id, password } = req.body;
  
  // db.json 파일에서 사용자 정보 읽어오기
  fs.readFile('./db.json', 'utf8', (err, data) => {
    if (err) {
      console.error('파일 읽기 에러:', err);
      return res.status(500).json({ message: '서버 에러' });
    }
    
    const users = JSON.parse(data).users;
    const user = users.find(u => u.id === id && u.password === password);
    
    if (!user) {
      return res.status(401).json({ success: false, message: '아이디 혹은 비밀번호가 틀렸습니다.' });
    }
    
    // 로그인 성공 시 JWT 토큰 발급 (토큰에 userId 포함)
    const token = jwt.sign({ userId: user.uid }, secretKey, { expiresIn: '1h' });
    
    // 반환 정보: success, token, userId
    return res.json({
      success: true,
      token: token,
      uid: user.uid,
      username: user.username
    });
  });
});

// 토큰 검증 미들웨어
function verifyToken(req, res, next) {
  const bearerHeader = req.headers['authorization'];
  if (!bearerHeader) {
    return res.status(403).json({ message: '토큰이 제공되지 않았습니다.' });
  }
  
  const token = bearerHeader.split(' ')[1]; // "Bearer {token}" 형식
  jwt.verify(token, secretKey, (err, decoded) => {
    if (err) {
      return res.status(401).json({ message: '유효하지 않은 토큰입니다.' });
    }
    // 토큰에 저장된 userId를 req 객체에 저장
    req.userId = decoded.userId;
    next();
  });
}


// 사용자 데이터
//app.post('/writeuserdata', verifyToken, (req, res) => {
//    const jsonData = req.body;
//    const uid = jsonData.userId;

//    if (!uid) {
//        jsonData.
//    }

//    fs.appendFile()
//});

app.post('/readuserdata', verifyToken, (req, res) => {
    const jsonData = req.body;
    const uid = jsonData.userId;

    if (!uid) {

    }

    fs.writeFile('')
});


// 캐릭터 소지 목록 반환 API (보호된 엔드포인트, POST 요청)
app.post('/characters', verifyToken, (req, res) => {
  fs.readFile('./characters.json', 'utf8', (err, data) => {
    if (err) {
      return res.status(500).json({ message: '서버 에러' });
    }
    
    let characterData;
    try {
      characterData = JSON.parse(data).characterData;
    } catch (e) {
      return res.status(500).json({ message: '데이터 파싱 에러' });
    }
    
    // 토큰에 포함된 userId로 해당 사용자의 데이터를 조회
    const userDataForUser = characterData.find(u => u.userId === req.userId);
    if (!userDataForUser) {
      return res.status(404).json({ message: '사용자 데이터를 찾을 수 없습니다.' });
    }
    
    return res.json({
      success: true,
      characters: userDataForUser.characters
    });
  });
});


// 캐릭터 정보 수정 및 업데이트 API (보호된 엔드포인트, POST 요청)
app.post('/updateheld', verifyToken, (req, res) => {
  // 클라이언트에서 수정된 캐릭터 배열을 전달 (예: [{ name: "전사", possessed: true }, ...])
  const newCharacters = req.body.characters;
  
  if (!Array.isArray(newCharacters)) {
    return res.status(400).json({ success: false, message: '캐릭터 배열 형식의 데이터가 필요합니다.' });
  }
  
  // character.json 파일 읽기
  fs.readFile('./character.json', 'utf8', (err, data) => {
    if (err) {
      console.error('파일 읽기 에러:', err);
      return res.status(500).json({ success: false, message: '서버 에러' });
    }
    
    let characterData;
    try {
      characterData = JSON.parse(data).characterData;
    } catch (e) {
      return res.status(500).json({ success: false, message: '데이터 파싱 에러' });
    }
    
    // 토큰에 포함된 userId를 이용하여 해당 사용자의 데이터 인덱스 찾기
    const userIndex = characterData.findIndex(u => u.userId === req.userId);
    if (userIndex === -1) {
      return res.status(404).json({ success: false, message: '사용자 데이터를 찾을 수 없습니다.' });
    }
    
    // 해당 사용자의 캐릭터 정보 업데이트
    characterData[userIndex].characters = newCharacters;
    
    // 수정된 데이터를 다시 character.json 파일에 저장
    const updatedData = { characterData: characterData };
    fs.writeFile('./character.json', JSON.stringify(updatedData, null, 2), 'utf8', (writeErr) => {
      if (writeErr) {
        console.error('파일 쓰기 에러:', writeErr);
        return res.status(500).json({ success: false, message: '데이터 저장 실패' });
      }
      
      // 업데이트된 캐릭터 정보를 반환
      return res.json({
        success: true,
        message: '캐릭터 정보 업데이트 성공',
        characters: newCharacters
      });
    });
  });
});

// 현재 시행중인 가챠 배너 제공
app.get('/banners', verifyToken, (req, res) => {
    fs.readFile('./banners.json', 'utf8', (err, data) => {
        if (err) {
            console.log("배너 서버 에러");
            return res.status(500).json({ message: '서버 에러' });
        }

        let bannersData;
        try {
            bannersData = JSON.parse(data).banners;
        } catch (e) {
            console.log("배너 파싱 에러");
            return res.status(500).json({ message: '데이터 파싱 에러' });
        }

        return res.json({
            banners: bannersData
        });
    });
});


// 가챠 진행 정보 로그 저장
app.post('/writegachalog', verifyToken, (req, res) => {
    const wrapper = req.body;

    if (!wrapper || !Array.isArray(wrapper.GachaResultList)) {
        console.log("유효하지 않은 데이터 형식이 들어옴(가챠로그)");
        return res.status(400).json({ message: '유효하지 않은 데이터 형식' });
    }

    let ndjson = "";
    wrapper.GachaResultList.forEach(result => {
        ndjson += JSON.stringify(result) + '\n';
    });

    const fileName = './gachalog_' + req.userId + '.json';

    //fs.access(fileName, fs.constants.F_OK, (accessErr) => {
    //    if (accessErr) {
    //        fs.writeFile(fileName, '', (writeErr) => {
    //            if (writeErr) {
    //                console.error('파일 생성 실패', writeErr);
    //                return res.status(500).json({ message: '새 파일 작성 실패' });
    //            }
    //        });
    //    }

    //    fs.appendFile(fileName, ndjson, (appendErr) => {
    //        if (appendErr) {
    //            console.error('로그 붙이기 실패', appendErr);
    //            return res.status(500).json({ message: '로그 붙이기 실패' });
    //        }
    //        res.json({ message: '로그 저장 완료' });
    //    });
    //}

    fs.appendFile(fileName, ndjson, (appendErr) => {
        if (appendErr) {
            console.error('로그 붙이기 실패', appendErr);
            return res.status(500).json({ message: '로그 붙이기 실패' });
        }
        res.json({ message: '로그 저장 완료' });
    });
});

app.get('/readgachalog', verifyToken, (req, res) => {
    fs.readFile('./gachalog_' + req.userId + '.json', 'utf8', (err, data) => {
        if (err) {
            return res.status(500).json({ message: '서버 에러' });
        }

        let bannersData;
        try {
            bannersData = JSON.parse(data).banners;
        } catch (e) {
            return res.status(500).json({ message: '데이터 파싱 에러' });
        }

        return res.json({
            banners: bannersData
        });
    });
});

app.listen(port, () => {
  console.log(`서버가 포트 ${port}에서 실행 중입니다.`);
});
